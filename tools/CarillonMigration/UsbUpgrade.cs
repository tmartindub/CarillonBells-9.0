using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace Carillon.Migration;

public sealed record UsbUpgradePlan(MigrationSource Source, string BackupFolder, IReadOnlyList<string> Warnings)
{
    internal byte[]? DatabaseFingerprint { get; init; }
}

public static partial class MigrationEngine
{
    private static readonly string UsbDatabaseEntry = Path.Combine("databases", "carillon.db");
    private const string UsbApplication = "CarillonBells.exe";
    private static readonly string[] LegacyApplicationFiles =
    [
        "Carillon.exe", UsbApplication, "CarillonMigration.exe", "CarillonDBUpgrade724To81.exe",
        "carillon.bat", "start_carillon.bat", "start_carillon.vbs", "Launch-Carillon-Evaluation.cmd",
        "libeay32.dll", "ssleay32.dll", "sqlite3.dll", "CarillonBells.dll", "CarillonBells.deps.json",
        "CarillonBells.runtimeconfig.json"
    ];

    public static UsbUpgradePlan PlanUsbUpgrade(MigrationSource source, string backupFolder, string executablePath)
    {
        var folder = FullPath(source.Folder);
        var backup = ValidateUsbBackupFolder(folder, backupFolder);
        ValidateUsbExecutable(executablePath);
        var database = ValidateUsbDatabase(source, folder);
        using var guard = ProtectDatabase(database);
        using var connection = OpenDatabase(database, SqliteOpenMode.ReadOnly, immutable: true);
        var data = InspectDatabase(connection);
        ConfirmSource(source, database, data);
        return new UsbUpgradePlan(new MigrationSource(folder, database, data.Version, data.Tracks.Count), backup,
            data.Warnings.ToArray()) { DatabaseFingerprint = SHA256.HashData(guard) };
    }

    public static MigrationResult UpgradeUsb(UsbUpgradePlan plan, string executablePath, IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var folder = FullPath(plan.Source.Folder);
        var backupFolder = ValidateUsbBackupFolder(folder, plan.BackupFolder);
        var executable = ValidateUsbExecutable(executablePath);
        var database = ValidateUsbDatabase(plan.Source, folder);
        var destinationExecutable = Path.Combine(folder, UsbApplication);
        CheckPath(destinationExecutable);
        if (Attributes(destinationExecutable) is { } destinationAttributes && destinationAttributes.HasFlag(FileAttributes.Directory))
            throw new IOException($"A directory already uses the name {UsbApplication} on this USB.");

        using var appLock = LockUsbFile(Path.Combine(folder, "Carillon.lock"));
        using var databaseLock = LockUsbFile(database + ".lock");
        var identifier = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N")[..8];
        var backup = Path.Combine(backupFolder, $"Carillon-before-USB-upgrade-{identifier}.zip");
        var report = Path.Combine(backupFolder, $"Carillon-USB-upgrade-{identifier}.txt");
        var temporaryReport = report + ".tmp";
        var stage = Path.Combine(folder, $".carillon-migration-{Guid.NewGuid():N}");
        var candidate = Path.Combine(stage, "candidate.db");
        var stagedExecutable = Path.Combine(stage, UsbApplication);
        var stageOwned = false;
        var temporaryReportOwned = false;
        var normalized = 0;
        try
        {
            SourceData data;
            Dictionary<string, byte[]> backupHashes;
            using (var guard = ProtectDatabase(database))
            using (var source = OpenDatabase(database, SqliteOpenMode.ReadOnly, immutable: true))
            {
                if (plan.DatabaseFingerprint is null || !CopyOrHash(guard, null, cancellationToken).SequenceEqual(plan.DatabaseFingerprint))
                    throw new IOException("The database changed after review. Review this USB again before upgrading.");
                data = InspectDatabase(source);
                ConfirmSource(plan.Source, database, data);

                Report("Creating and verifying the backup ZIP...");
                Directory.CreateDirectory(backupFolder);
                CheckPath(backupFolder);
                backupHashes = CreateUsbBackup(folder, backup, cancellationToken);

                Report("Preparing the USB upgrade...");
                if (!CreateStageDirectory(stage, IntPtr.Zero))
                    throw new IOException("The USB upgrade could not create a temporary folder on the selected drive.",
                        new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
                stageOwned = true;

                if (data.Version == LegacyVersion.Version724)
                {
                    using var snapshot = OpenDatabase(candidate, SqliteOpenMode.ReadWriteCreate);
                    source.BackupDatabase(snapshot);
                }
            }

            if (data.Version == LegacyVersion.Version724)
            {
                Report("Converting the 7.2.4 database copy...");
                var repository = new CarillonRepository(candidate);
                repository.EnsureCompatibleSchema();
                normalized = repository.NormalizeLegacyTimes();
                repository.Settings();
                repository.Silence();
                repository.Songs();
                using var converted = OpenDatabase(candidate, SqliteOpenMode.ReadOnly);
                var result = InspectDatabase(converted);
                if (result.Version != LegacyVersion.Version81 || !result.Tracks.SequenceEqual(data.Tracks) ||
                    !result.Rotation.SequenceEqual(data.Rotation))
                    throw new InvalidDataException("The converted database did not retain the expected playlist and rotation slots.");
            }

            var installExecutable = !SamePath(executable, destinationExecutable);
            if (installExecutable)
            {
                Report("Copying and verifying the v9 executable...");
                var transfer = new Transfer();
                AddFile(transfer, executable, UsbApplication);
                CopyTransfer(transfer, stage, cancellationToken);
            }

            Report("Installing v9 on this USB...");
            CheckPath(stage);
            CheckPath(backupFolder);
            var existingExecutable = backupHashes.GetValueOrDefault(UsbApplication);

            using (var reportStream = new FileStream(temporaryReport, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                temporaryReportOwned = true;
                using var writer = new StreamWriter(reportStream);
                writer.WriteLine("Carillon Bells 9.0 USB upgrade");
                writer.WriteLine($"Installation folder: {folder}");
                writer.WriteLine($"Verified pre-upgrade ZIP: {backup}");
                writer.WriteLine($"Playlist entries: {data.Tracks.Count}");
                if (data.Version == LegacyVersion.Version724)
                    writer.WriteLine($"Converted the 7.2.4 database. Legacy time values normalized: {normalized}");
                else
                    writer.WriteLine("The existing 8.1 database was retained byte for byte during this upgrade.");
                writer.WriteLine("Music and legacy launchers stayed in their existing folders.");
                foreach (var warning in data.Warnings) writer.WriteLine(warning);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var executableInstalled = false;
            var databaseInstalled = false;
            try
            {
                using (var guard = ProtectDatabase(database))
                {
                    if (!CopyOrHash(guard, null, cancellationToken).SequenceEqual(backupHashes[UsbDatabaseEntry]))
                        throw new IOException("The USB database changed after the backup was taken.");
                    VerifyUsbOriginal(destinationExecutable, existingExecutable, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (installExecutable)
                    {
                        File.Move(stagedExecutable, destinationExecutable, overwrite: existingExecutable is not null);
                        executableInstalled = true;
                    }
                }
                if (data.Version == LegacyVersion.Version724)
                {
                    File.Move(candidate, database, overwrite: true);
                    databaseInstalled = true;
                }
                File.Move(temporaryReport, report);
                temporaryReportOwned = false;
            }
            catch (Exception installError)
            {
                if (!databaseInstalled && !executableInstalled) throw;
                try
                {
                    using var zip = ZipFile.OpenRead(backup);
                    if (databaseInstalled) RestoreUsbOriginal(zip, UsbDatabaseEntry, database, stage, backupHashes[UsbDatabaseEntry]);
                    if (executableInstalled) RestoreUsbOriginal(zip, UsbApplication, destinationExecutable, stage, existingExecutable);
                }
                catch (Exception restoreError)
                {
                    throw new IOException($"The USB upgrade did not finish and automatic rollback could not complete. Keep the verified backup at {backup}.",
                        new AggregateException(installError, restoreError));
                }
                throw;
            }

            return new MigrationResult(folder, backup, report, 0, data.Warnings.ToArray());
        }
        finally
        {
            try
            {
                if (stageOwned)
                {
                    CheckPath(stage);
                    Directory.Delete(stage, recursive: true);
                }
            }
            finally
            {
                if (temporaryReportOwned)
                {
                    CheckPath(temporaryReport);
                    File.Delete(temporaryReport);
                }
            }
        }

        void Report(string message)
        {
            progress?.Report(message);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static string ValidateUsbDatabase(MigrationSource source, string folder)
    {
        var database = Path.Combine(folder, UsbDatabaseEntry);
        CheckPath(database);
        if (!SamePath(FullPath(source.DatabasePath), database) || !File.Exists(database))
            throw new IOException(@"Upgrading this USB requires its working database at databases\carillon.db.");
        return database;
    }

    private static string ValidateUsbExecutable(string executablePath)
    {
        var executable = FullPath(executablePath);
        CheckPath(executable);
        if (!SamePath(Path.GetFileName(executable), UsbApplication) || !File.Exists(executable))
            throw new FileNotFoundException("The bundled CarillonBells.exe could not be found.");
        return executable;
    }

    private static string ValidateUsbBackupFolder(string folder, string backupFolder)
    {
        if (!Path.IsPathFullyQualified(backupFolder))
            throw new IOException("Choose a full path for the backup folder on the computer or another drive.");
        var backup = FullPath(backupFolder);
        CheckPath(backup);
        var sourceRoot = Path.GetPathRoot(folder)!;
        if (SamePath(backup, folder) || IsInside(backup, folder) ||
            (SamePath(sourceRoot, Path.GetPathRoot(backup)!) && new DriveInfo(sourceRoot).DriveType == DriveType.Removable))
            throw new IOException("Choose a backup folder outside the USB being upgraded.");
        if (!Directory.Exists(backup) && (Attributes(backup) is not null || Path.GetDirectoryName(backup) is not { } parent ||
            !Directory.Exists(parent)))
            throw new DirectoryNotFoundException("Select an existing backup folder or a new folder within an existing location.");
        return backup;
    }

    private static FileStream LockUsbFile(string path)
    {
        CheckPath(path);
        try
        {
            if (Attributes(path) is null)
                return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException error)
        {
            throw new IOException("Close every Carillon window and any other program using its database before upgrading this USB.", error);
        }
    }

    private static Dictionary<string, byte[]> CreateUsbBackup(string folder, string backup, CancellationToken cancellationToken)
    {
        var hashes = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        var created = false;
        var verified = false;
        try
        {
            using (var output = new FileStream(backup, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                created = true;
                using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var relative in LegacyApplicationFiles.Prepend(UsbDatabaseEntry))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var path = Path.Combine(folder, relative);
                        CheckPath(path);
                        if (!File.Exists(path)) continue;
                        using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var entry = zip.CreateEntry(ZipEntryName(relative), CompressionLevel.Fastest).Open();
                        hashes.Add(relative, CopyOrHash(input, entry, cancellationToken));
                    }
                }
                output.Flush(flushToDisk: true);
            }

            using (var zip = ZipFile.OpenRead(backup))
            {
                if (!hashes.ContainsKey(UsbDatabaseEntry) || zip.Entries.Count != hashes.Count)
                    throw new InvalidDataException("The pre-upgrade ZIP did not contain the required database and application files.");
                foreach (var (relative, hash) in hashes)
                {
                    var entry = zip.GetEntry(ZipEntryName(relative)) ?? throw new InvalidDataException("The pre-upgrade ZIP is incomplete.");
                    using var contents = entry.Open();
                    if (!CopyOrHash(contents, null, cancellationToken).SequenceEqual(hash))
                        throw new InvalidDataException("The pre-upgrade ZIP could not be verified.");
                }
            }
            verified = true;
            return hashes;
        }
        finally
        {
            if (created && !verified) File.Delete(backup);
        }
    }

    private static void VerifyUsbOriginal(string path, byte[]? expected, CancellationToken cancellationToken)
    {
        CheckPath(path);
        if (expected is null && Attributes(path) is null) return;
        if (expected is not null && File.Exists(path))
        {
            using var original = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (CopyOrHash(original, null, cancellationToken).SequenceEqual(expected)) return;
        }
        throw new IOException($"A USB file changed after the backup was taken: {Path.GetFileName(path)}.");
    }

    private static void RestoreUsbOriginal(ZipArchive zip, string relative, string destination, string stage, byte[]? expected)
    {
        CheckPath(destination);
        if (expected is null)
        {
            File.Delete(destination);
            return;
        }

        var restored = Path.Combine(stage, $"restore-{Path.GetFileName(relative)}");
        using (var input = (zip.GetEntry(ZipEntryName(relative)) ?? throw new InvalidDataException("The rollback file is missing.")).Open())
        using (var output = new FileStream(restored, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            if (!CopyOrHash(input, output, CancellationToken.None).SequenceEqual(expected))
                throw new InvalidDataException("The rollback file could not be verified.");
            output.Flush(flushToDisk: true);
        }
        File.Move(restored, destination, overwrite: true);
    }

    private static string ZipEntryName(string relative) => relative.Replace(Path.DirectorySeparatorChar, '/');
}
