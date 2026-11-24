using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace Carillon.Migration;

public enum LegacyVersion { Version724, Version81 }

public sealed record MigrationSource(string Folder, string DatabasePath, LegacyVersion Version, int PlaylistCount);
public sealed record MigrationPlan(MigrationSource Source, string Destination, int MusicFileCount, long MusicBytes,
    IReadOnlyList<string> Warnings)
{
    internal byte[]? SourceFingerprint { get; init; }
}
public sealed record MigrationResult(string Destination, string BackupPath, string ReportPath, int MusicFilesCopied,
    IReadOnlyList<string> Warnings);

public static partial class MigrationEngine
{
    private sealed record Rotation(int Slot, string Directory);
    private sealed record SourceData(LegacyVersion Version, List<string> Tracks, List<Rotation> Rotation, List<string> Warnings);
    private sealed record TransferFile(string Source, long Length);

    private sealed class Transfer
    {
        public Dictionary<string, TransferFile> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private static readonly string[] MusicDirectories = ["Music", "Songs", "Random_songs", "Random_songs-2"];
    private static readonly string[] SupportDirectories = ["help", "docs", "third-party-notices"];
    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".wma", ".aac", ".m4a", ".m4b", ".mp4", ".flac", ".ogg", ".opus", ".aif", ".aiff", ".mid", ".midi"
    };
    private static readonly string[] ReservedDirectories = ["databases", "data", "logs", .. SupportDirectories];
    private const string BackupFile = "carillon_before_migration.zip";
    private const string ReportFile = "migration-report.txt";
    private const string RotationTable = "PL_RAND_DIR_ROTATION";

    [DllImport("kernel32.dll", EntryPoint = "CreateDirectoryW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateStageDirectory(string path, IntPtr attributes);

    public static MigrationSource InspectSource(string sourceFolder)
    {
        var folder = FullPath(sourceFolder);
        var database = FindDatabase(folder);
        using var guard = ProtectDatabase(database);
        using var source = OpenDatabase(database, SqliteOpenMode.ReadOnly, immutable: true);
        var data = InspectDatabase(source);
        return new MigrationSource(folder, database, data.Version, data.Tracks.Count);
    }

    public static MigrationPlan Plan(MigrationSource source, string destination, string templateFolder)
    {
        var folder = FullPath(source.Folder);
        var template = FullPath(templateFolder);
        var target = ValidateDestination(folder, destination, template);
        var database = FindDatabase(folder);
        using var guard = ProtectDatabase(database);
        using var connection = OpenDatabase(database, SqliteOpenMode.ReadOnly, immutable: true);
        var data = InspectDatabase(connection);
        ConfirmSource(source, database, data);
        var music = PlanMusic(folder, target, data, CancellationToken.None);
        PlanTemplate(template, CancellationToken.None);
        return new MigrationPlan(new MigrationSource(folder, database, data.Version, data.Tracks.Count), target,
            music.Files.Count, music.Files.Values.Sum(file => file.Length), data.Warnings.ToArray())
        {
            SourceFingerprint = SHA256.HashData(guard)
        };
    }

    public static MigrationResult Run(MigrationPlan plan, string templateFolder, IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var folder = FullPath(plan.Source.Folder);
        var template = FullPath(templateFolder);
        var target = ValidateDestination(folder, plan.Destination, template);
        var application = PlanTemplate(template, cancellationToken);
        var parent = Path.GetDirectoryName(target)!;
        var stage = Path.Combine(parent, $".carillon-migration-{Guid.NewGuid():N}");
        var backupRelative = Path.Combine("databases", "backup", BackupFile);
        var stageOwned = false;

        try
        {
            CheckPath(parent);
            if (!CreateStageDirectory(stage, IntPtr.Zero))
                throw new IOException("The temporary migration folder could not be created.",
                    new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
            stageOwned = true;
            var database = Path.Combine(stage, "databases", "carillon.db");
            Directory.CreateDirectory(Path.GetDirectoryName(database)!);

            Report("Taking a read-only database snapshot...");
            var sourceDatabase = FindDatabase(folder);
            using (var guard = ProtectDatabase(sourceDatabase))
            {
                if (plan.SourceFingerprint is null || !CopyOrHash(guard, null, cancellationToken).SequenceEqual(plan.SourceFingerprint))
                    throw new IOException("The source database changed since review. Select and review it again.");
                using var source = OpenDatabase(sourceDatabase, SqliteOpenMode.ReadOnly, immutable: true);
                using var candidate = OpenDatabase(database, SqliteOpenMode.ReadWriteCreate);
                source.BackupDatabase(candidate);
            }

            SourceData data;
            using (var candidate = OpenDatabase(database, SqliteOpenMode.ReadOnly))
                data = InspectDatabase(candidate);
            ConfirmSource(plan.Source, sourceDatabase, data);

            Report("Creating and verifying the pre-conversion ZIP...");
            WriteBackup(database, Path.Combine(stage, backupRelative), cancellationToken);

            var normalized = 0;
            if (data.Version == LegacyVersion.Version724)
            {
                Report("Converting the 7.2.4 database for v9...");
                var repository = new CarillonRepository(database);
                repository.EnsureCompatibleSchema();
                normalized = repository.NormalizeLegacyTimes();
                repository.Settings();
                repository.Silence();
                repository.Songs();
                using var candidate = OpenDatabase(database, SqliteOpenMode.ReadOnly);
                var converted = InspectDatabase(candidate);
                if (converted.Version != LegacyVersion.Version81 || !converted.Tracks.SequenceEqual(data.Tracks) ||
                    !converted.Rotation.SequenceEqual(data.Rotation))
                    throw new InvalidDataException("The converted database did not retain the expected playlist and rotation slots.");
            }

            var music = PlanMusic(folder, target, data, cancellationToken);
            if (music.Files.Count != plan.MusicFileCount || music.Files.Values.Sum(file => file.Length) != plan.MusicBytes ||
                !data.Warnings.SequenceEqual(plan.Warnings))
                throw new IOException("The migration contents changed since review. Review the source and destination again.");

            Report("Copying application files...");
            CopyTransfer(application, stage, cancellationToken);
            Report("Copying music files...");
            CopyTransfer(music, stage, cancellationToken);

            Report("Validating the new portable folder...");
            using (var candidate = OpenDatabase(database, SqliteOpenMode.ReadOnly))
                CheckIntegrity(candidate);
            WriteReport(Path.Combine(stage, ReportFile), plan, data, normalized, music.Files.Count, backupRelative);
            ValidateDestination(folder, target, template);
            CheckPath(stage);
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Move(stage, target);
            stageOwned = false;

            return new MigrationResult(target, Path.Combine(target, backupRelative), Path.Combine(target, ReportFile),
                music.Files.Count, data.Warnings.ToArray());
        }
        finally
        {
            if (stageOwned)
            {
                CheckPath(stage);
                Directory.Delete(stage, recursive: true);
            }
        }

        void Report(string message)
        {
            progress?.Report(message);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static string FindDatabase(string folder)
    {
        CheckPath(folder);
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException("Select an existing Carillon installation folder.");
        foreach (var path in new[] { Path.Combine(folder, "databases", "carillon.db"), Path.Combine(folder, "carillon.db") })
        {
            CheckPath(path);
            if (Attributes(path) is { } attributes && !attributes.HasFlag(FileAttributes.Directory))
                return path;
        }
        throw new FileNotFoundException("No carillon.db was found in the selected folder or its databases folder.");
    }

    private static FileStream ProtectDatabase(string database)
    {
        CheckPath(database);
        var guard = new FileStream(database, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
            {
                var sidecar = database + suffix;
                CheckPath(sidecar);
                if (Attributes(sidecar) is { } attributes && (attributes.HasFlag(FileAttributes.Directory) ||
                    suffix == "-shm" || new FileInfo(sidecar).Length > 0))
                    throw new IOException("SQLite recovery or transaction files are present. Close Carillon and any database tools, then try again.");
            }
            return guard;
        }
        catch
        {
            guard.Dispose();
            throw;
        }
    }

    private static SqliteConnection OpenDatabase(string database, SqliteOpenMode mode, bool immutable = false)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = immutable ? new Uri(database).AbsoluteUri + "?immutable=1" : database,
            Mode = mode,
            Pooling = false,
            DefaultTimeout = 5
        }.ToString());
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static SourceData InspectDatabase(SqliteConnection connection)
    {
        CheckIntegrity(connection);
        RequireFields(connection, "playlist", ["playlist_name", "song_name", "season", "song_duration", "play_date_from",
            "play_date_to", "num_times_to_play", .. Song.DatabaseDays.Select(name => "play_" + name),
            .. Enumerable.Range(1, 12).Select(number => "scheduled_time" + number)]);
        RequireFields(connection, "seasonalgroups", ["SEASONALGROUP", "PLAY_DATE_FROM", "PLAY_DATE_TO", "PLAY_TIME"]);
        if (Fields(connection, "PL_SETTINGS").Count == 0)
            throw new InvalidDataException("The database is missing the Carillon settings table.");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM PL_SETTINGS";
        var settingsCount = Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        if (settingsCount > 1)
            throw new InvalidDataException("The database has more than one settings row and requires review before migration.");

        var warnings = new List<string>();
        if (settingsCount == 0)
            warnings.Add("No saved settings were found. Carillon supplies defaults when preparing this database for v9.");

        var rotationFields = Fields(connection, RotationTable);
        var version = rotationFields.Contains("slot_no") ? LegacyVersion.Version81 : LegacyVersion.Version724;
        var rotations = new List<Rotation>();
        if (version == LegacyVersion.Version81)
        {
            RequireFields(connection, RotationTable, ["slot_no", "rand_music_dir", "frm_date", "to_date"]);
            command.CommandText = $"SELECT slot_no, rand_music_dir, frm_date, to_date FROM {RotationTable} ORDER BY slot_no";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var slot = reader.GetInt32(0);
                if (slot is < 1 or > 7 || rotations.Any(rotation => rotation.Slot == slot))
                    throw new InvalidDataException("The random music table contains invalid or duplicate slot numbers.");
                rotations.Add(new Rotation(slot, ReadText(reader, 1)));
                DataFormats.Date(reader.GetValue(2));
                DataFormats.Date(reader.GetValue(3));
            }
        }
        else
        {
            var columns = Enumerable.Range(1, 7).SelectMany(number =>
                new[] { $"rand_music_dir{number}", $"frm_date{number}", $"to_date{number}" }).ToArray();
            RequireFields(connection, RotationTable, ["id", .. columns]);
            command.CommandText = $"SELECT id, {string.Join(", ", columns)} FROM {RotationTable}";
            using var reader = command.ExecuteReader();
            if (!reader.Read() || reader.GetInt32(0) != 1)
                throw new InvalidDataException("The older random music table must contain the original settings row with ID 1.");
            foreach (var slot in Enumerable.Range(1, 7))
            {
                var index = (slot - 1) * 3 + 1;
                rotations.Add(new Rotation(slot, ReadText(reader, index)));
                DataFormats.Date(reader.GetValue(index + 1));
                DataFormats.Date(reader.GetValue(index + 2));
            }
            if (reader.Read())
                throw new InvalidDataException("The older random music table contains multiple settings rows.");
        }

        command.CommandText = "SELECT PLAY_DATE_FROM, PLAY_DATE_TO, PLAY_TIME FROM seasonalgroups";
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                DataFormats.Date(reader.GetValue(0));
                DataFormats.Date(reader.GetValue(1));
                DataFormats.Time(reader.GetValue(2));
            }
        }

        var silence = Fields(connection, "silence_schedule");
        if (silence.Count > 0)
        {
            RequireFields(connection, "silence_schedule", ["silence_id", "enabled", "start_time", "end_time", .. Song.DatabaseDays]);
            command.CommandText = "SELECT start_time, end_time FROM silence_schedule";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                DataFormats.Time(reader.GetValue(0));
                DataFormats.Time(reader.GetValue(1));
            }
        }
        else if (version == LegacyVersion.Version81)
            warnings.Add("No silence windows are stored. Carillon v9 will initialize them when it first opens the database.");

        var tracks = new List<string>();
        var times = string.Join(", ", Enumerable.Range(1, 12).Select(number => "scheduled_time" + number));
        command.CommandText = $"SELECT song_name, play_date_from, play_date_to, {times} FROM playlist ORDER BY song_name";
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                tracks.Add(ReadText(reader, 0));
                DataFormats.Date(reader.GetValue(1));
                DataFormats.Date(reader.GetValue(2));
                for (var index = 3; index < reader.FieldCount; index++) DataFormats.Time(reader.GetValue(index));
            }
        }
        return new SourceData(version, tracks, rotations, warnings);
    }

    private static void ConfirmSource(MigrationSource expected, string database, SourceData data)
    {
        if (!SamePath(FullPath(expected.DatabasePath), database) || expected.Version != data.Version ||
            expected.PlaylistCount != data.Tracks.Count)
            throw new IOException("The source database changed since selection. Select and review it again.");
    }

    private static void RequireFields(SqliteConnection connection, string table, string[] required)
    {
        var fields = Fields(connection, table);
        var missing = required.Where(field => !fields.Contains(field)).ToArray();
        if (missing.Length > 0)
            throw new InvalidDataException($"Unsupported Carillon database. {table} is missing: {string.Join(", ", missing)}.");
    }

    private static HashSet<string> Fields(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info([{table}])";
        using var reader = command.ExecuteReader();
        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read()) fields.Add(reader.GetString(1));
        return fields;
    }

    private static string ReadText(SqliteDataReader reader, int column) =>
        reader.IsDBNull(column) ? "" : Convert.ToString(reader.GetValue(column), CultureInfo.InvariantCulture) ?? "";

    private static void CheckIntegrity(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA quick_check";
        using var reader = command.ExecuteReader();
        if (!reader.Read() || !reader.GetString(0).Equals("ok", StringComparison.OrdinalIgnoreCase) || reader.Read())
            throw new InvalidDataException("The database did not pass the SQLite integrity check.");
    }

    private static string ValidateDestination(string source, string destination, string template)
    {
        var target = FullPath(destination);
        var parent = Path.GetDirectoryName(target);
        if (parent is null)
            throw new IOException("Enter a new folder name inside an existing destination folder.");
        CheckPath(target);
        if (!Directory.Exists(parent))
            throw new DirectoryNotFoundException("The parent of the new portable folder does not exist.");
        if (Attributes(target) is not null)
            throw new IOException("The new portable folder must not already exist.");
        if (SamePath(target, source) || IsInside(target, template) || SamePath(target, template))
            throw new IOException("Choose a destination outside the source installation and v9 application folders.");
        if (IsInside(target, source) && (!SamePath(source, Path.GetPathRoot(source)!) || !SamePath(parent, source) ||
            MusicDirectories.Concat(ReservedDirectories).Any(name => SamePath(name, Path.GetFileName(target)))))
            throw new IOException("Choose a destination outside the old installation, or a new folder directly on its drive when the old installation is at the drive root.");
        return target;
    }

    private static Transfer PlanTemplate(string template, CancellationToken cancellationToken)
    {
        CheckPath(template);
        var application = Path.Combine(template, "CarillonBells.exe");
        CheckPath(application);
        if (!File.Exists(application))
            throw new FileNotFoundException("The migration wizard must run from the packaged v9 application folder.");

        var transfer = new Transfer();
        foreach (var file in Directory.EnumerateFiles(template).Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = Path.GetFileName(file);
            if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase) ||
                new[] { "README.md", "LICENSE", "NOTICE", "Launch-Carillon-Evaluation.cmd" }.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                CheckPath(file);
                AddFile(transfer, file, name);
            }
        }

        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(culture => culture.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var directory in Directory.EnumerateDirectories(template).Order(StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(directory);
            if (SupportDirectories.Contains(name, StringComparer.OrdinalIgnoreCase))
                AddTree(transfer, directory, name, file => new[] { ".htm", ".html", ".css", ".js", ".png", ".jpg", ".jpeg",
                    ".svg", ".gif", ".webp", ".ico", ".pdf", ".docx", ".md", ".txt" }.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase),
                    true, null, cancellationToken);
            else if (cultures.Contains(name))
                AddTree(transfer, directory, name, file => file.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase),
                    false, null, cancellationToken);
        }
        return transfer;
    }

    private static Transfer PlanMusic(string source, string target, SourceData data, CancellationToken cancellationToken)
    {
        var transfer = new Transfer();
        foreach (var name in MusicDirectories)
        {
            var directory = Path.Combine(source, name);
            if (Attributes(directory) is { } attributes && attributes.HasFlag(FileAttributes.Directory))
                AddTree(transfer, directory, name, IsAudio, true, data.Warnings, cancellationToken);
        }

        foreach (var path in data.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Path.GetFileNameWithoutExtension(path).Contains("Random Music Generator", StringComparison.OrdinalIgnoreCase)) continue;
            if (!MapMusicPath(source, target, path, out var file, out var relative) || !IsAudio(file) || !File.Exists(file))
            {
                data.Warnings.Add($"Playlist track unavailable: {path}");
                continue;
            }
            AddFile(transfer, file, relative);
        }

        if (!data.Rotation.Any(item => item.Slot == 1 && !string.IsNullOrWhiteSpace(item.Directory)))
            data.Warnings.Add("Random music slot 1 has no fallback directory configured.");
        foreach (var item in data.Rotation.Where(item => !string.IsNullOrWhiteSpace(item.Directory)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!MapMusicPath(source, target, item.Directory, out var directory, out var relative) || !Directory.Exists(directory))
            {
                data.Warnings.Add($"Random music slot {item.Slot} directory unavailable: {item.Directory}");
                continue;
            }
            var rootDirectory = SamePath(source, directory);
            if (rootDirectory)
                data.Warnings.Add($"Random music slot {item.Slot} points to the source folder. Only its top-level audio is copied for this slot.");
            AddTree(transfer, directory, relative, IsAudio, !rootDirectory, data.Warnings, cancellationToken);
            if (!transfer.Files.Keys.Any(file => SamePath(Path.GetDirectoryName(file) ?? "", relative == "." ? "" : relative) &&
                Path.GetExtension(file).Equals(".mp3", StringComparison.OrdinalIgnoreCase)))
                data.Warnings.Add($"Random music slot {item.Slot} has no playable MP3 files at the top of its directory.");
        }
        return transfer;
    }

    private static bool MapMusicPath(string source, string target, string saved, out string sourcePath, out string relative)
    {
        sourcePath = "";
        relative = "";
        if (string.IsNullOrWhiteSpace(saved)) return false;
        try
        {
            sourcePath = DataFormats.PortablePath(source, saved);
            relative = Path.GetRelativePath(target, DataFormats.PortablePath(target, saved));
            var sourceRelative = Path.GetRelativePath(source, sourcePath);
            if (ProtectedPath(sourceRelative) || ProtectedPath(relative) || IsInside(sourcePath, target) || SamePath(sourcePath, target))
                return false;
            CheckPath(sourcePath);
            return true;
        }
        catch (ArgumentException) { return false; }
        catch (NotSupportedException) { return false; }
        catch (PathTooLongException) { return false; }
        catch (InvalidDataException) { return false; }
    }

    private static bool ProtectedPath(string relative)
    {
        var first = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return relative.Contains(':') || first == ".." || first.StartsWith(".carillon-migration-", StringComparison.OrdinalIgnoreCase) ||
            ReservedDirectories.Contains(first, StringComparer.OrdinalIgnoreCase) || SamePath(first, ReportFile);
    }

    private static bool IsAudio(string file) => AudioExtensions.Contains(Path.GetExtension(file));

    private static void AddTree(Transfer transfer, string source, string relative, Func<string, bool> includeFile, bool recursive,
        List<string>? warnings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Attributes(source) is { } directoryAttributes && directoryAttributes.HasFlag(FileAttributes.ReparsePoint))
        {
            SkipLink(source);
            return;
        }

        if (relative != ".") transfer.Directories.Add(relative);
        foreach (var path in Directory.EnumerateFileSystemEntries(source).Order(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var attributes = File.GetAttributes(path);
            var target = Path.Combine(relative == "." ? "" : relative, Path.GetFileName(path));
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                if (recursive) AddTree(transfer, path, target, includeFile, recursive, warnings, cancellationToken);
            }
            else if (includeFile(path))
            {
                if (attributes.HasFlag(FileAttributes.ReparsePoint)) SkipLink(path);
                else AddFile(transfer, path, target);
            }
        }

        void SkipLink(string path)
        {
            if (warnings is null)
                throw new InvalidDataException($"The v9 application package contains a linked path: {path}");
            warnings.Add($"Skipped linked music path: {path}");
        }
    }

    private static void AddFile(Transfer transfer, string source, string relative)
    {
        if (transfer.Files.TryGetValue(relative, out var existing))
        {
            if (!SamePath(existing.Source, source))
                throw new IOException($"More than one source file maps to the same destination: {relative}");
            return;
        }
        transfer.Files.Add(relative, new TransferFile(source, new FileInfo(source).Length));
    }

    private static void CopyTransfer(Transfer transfer, string stage, CancellationToken cancellationToken)
    {
        foreach (var relative in transfer.Directories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(TransferPath(stage, relative));
        }
        foreach (var (relative, file) in transfer.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CheckPath(file.Source);
            var destination = TransferPath(stage, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            byte[] expected;
            using (var input = new FileStream(file.Source, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (input.Length != file.Length)
                    throw new IOException($"A source file changed since review: {relative}");
                using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                expected = CopyOrHash(input, output, cancellationToken);
                output.Flush(flushToDisk: true);
            }
            using var verification = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (!expected.SequenceEqual(CopyOrHash(verification, null, cancellationToken)))
                throw new IOException($"The copied file could not be verified: {relative}");
        }
    }

    private static string TransferPath(string stage, string relative)
    {
        var path = FullPath(Path.Combine(stage, relative));
        if (!IsInside(path, stage))
            throw new InvalidDataException("A migration path points outside the temporary folder.");
        CheckPath(path);
        return path;
    }

    private static byte[] CopyOrHash(Stream input, Stream? output, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[128 * 1024];
        var count = 0;
        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hash.AppendData(buffer, 0, count);
            output?.Write(buffer, 0, count);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return hash.GetHashAndReset();
    }

    private static void WriteBackup(string database, string backup, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
        byte[] expected;
        using (var input = new FileStream(database, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var output = new FileStream(backup, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            using (var entry = zip.CreateEntry("carillon.db", CompressionLevel.Optimal).Open())
                expected = CopyOrHash(input, entry, cancellationToken);
            output.Flush(flushToDisk: true);
        }
        using var verification = ZipFile.OpenRead(backup);
        using var contents = verification.GetEntry("carillon.db")!.Open();
        if (!expected.SequenceEqual(CopyOrHash(contents, null, cancellationToken)))
            throw new InvalidDataException("The pre-conversion database ZIP could not be verified.");
    }

    private static void WriteReport(string path, MigrationPlan plan, SourceData data, int normalized, int files, string backup)
    {
        using var output = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None));
        output.WriteLine(App.ProductTitle + " - Migration report");
        output.WriteLine(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture));
        output.WriteLine();
        output.WriteLine($"Source version: {(data.Version == LegacyVersion.Version724 ? "7.2.4" : "8.1")}");
        output.WriteLine($"Source folder: {plan.Source.Folder}");
        output.WriteLine($"Destination: {plan.Destination}");
        output.WriteLine($"Playlist entries: {data.Tracks.Count}");
        output.WriteLine($"Music files copied and verified: {files}");
        if (data.Version == LegacyVersion.Version724)
            output.WriteLine($"Legacy time values normalized: {normalized}");
        else
            output.WriteLine("The 8.1 database was preserved as a SQLite snapshot for v9 startup.");
        output.WriteLine($"Verified pre-conversion backup: {backup}");
        output.WriteLine();
        if (data.Warnings.Count == 0)
            output.WriteLine("No migration warnings.");
        else
        {
            output.WriteLine("Review these items before running the new installation:");
            foreach (var warning in data.Warnings) output.WriteLine("- " + warning);
        }
    }

    private static string FullPath(string path)
    {
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        if (full.StartsWith(@"\\?\", StringComparison.Ordinal) || full.StartsWith(@"\\.\", StringComparison.Ordinal))
            throw new ArgumentException("Choose a regular Windows path for the migration folders.");
        return full;
    }
    private static bool SamePath(string first, string second) => first.Equals(second, StringComparison.OrdinalIgnoreCase);
    private static bool IsInside(string path, string folder) => !SamePath(path, folder) && path.StartsWith(
        Path.EndsInDirectorySeparator(folder) ? folder : folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static FileAttributes? Attributes(string path)
    {
        try { return File.GetAttributes(path); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private static void CheckPath(string path)
    {
        var parts = new Stack<string>();
        for (string? current = path; current is not null; current = Path.GetDirectoryName(current)) parts.Push(current);
        foreach (var current in parts)
            if (Attributes(current) is { } attributes && attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidDataException($"Migration cannot follow this linked path: {current}");
    }
}
