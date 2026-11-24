using System.Globalization;
using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace Carillon.Migration;

public sealed record RestoreBackup(string ZipPath, DateTime BackupDate);

public sealed record Restore724Plan(string InstallationFolder, string DatabasePath, string BackupPath,
    DateTime BackupDate, int PlaylistCount)
{
    internal byte[]? CurrentFingerprint { get; init; }
    internal byte[]? BackupFingerprint { get; init; }
}

public sealed record Restore724Result(string DatabasePath, string PreRestoreBackupPath, string? LegacyExecutablePath);

public static partial class MigrationEngine
{
    public static IReadOnlyList<RestoreBackup> DiscoverRestoreBackups(string folder)
    {
        var database = ResolveRestoreDatabase(folder);
        return Directory.EnumerateFiles(Path.GetDirectoryName(database)!, "carillon_before_upgrade_*.zip")
            .Where(path => Attributes(path) is { } attributes && !attributes.HasFlag(FileAttributes.ReparsePoint))
            .Select(path => new RestoreBackup(path, File.GetLastWriteTime(path)))
            .OrderByDescending(item => item.BackupDate)
            .ThenByDescending(item => item.ZipPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static Restore724Plan PlanRestore724(string folder, string zip, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = ResolveRestoreDatabase(folder);
        var installation = FullPath(folder);
        if (!Path.IsPathFullyQualified(zip) || !Path.GetExtension(zip).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Select the ZIP created before the 7.2.4 upgrade.");
        var archive = FullPath(zip);
        byte[] currentFingerprint;
        using (var guard = ProtectDatabase(database))
        using (var current = OpenDatabase(database, SqliteOpenMode.ReadOnly, immutable: true))
        {
            CheckRestoreTarget(current);
            currentFingerprint = CopyOrHash(guard, null, cancellationToken);
        }

        var stage = CreateRestoreStage(Path.GetTempPath());
        var candidate = Path.Combine(stage, "restore.db");
        try
        {
            var backup = StageRestoreDatabase(archive, candidate, cancellationToken);
            return new Restore724Plan(installation, database, archive, backup.Date, backup.Count)
            {
                CurrentFingerprint = currentFingerprint,
                BackupFingerprint = backup.Hash
            };
        }
        finally
        {
            File.Delete(candidate);
            Directory.Delete(stage);
        }
    }

    public static Restore724Result Restore724(Restore724Plan plan, IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = ResolveRestoreDatabase(plan.InstallationFolder);
        if (!SamePath(database, plan.DatabasePath) || plan.CurrentFingerprint is null || plan.BackupFingerprint is null)
            throw new InvalidDataException("Review the installation and backup ZIP before restoring.");

        var folder = Path.GetDirectoryName(database)!;
        using var appLock = LockUsbFile(Path.Combine(plan.InstallationFolder, "Carillon.lock"));
        using var databaseLock = LockUsbFile(database + ".lock");
        var stage = CreateRestoreStage(folder);
        var candidate = Path.Combine(stage, "restore.db");
        var temporaryBackup = Path.Combine(stage, "before-restore.zip");
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
        var savedBackup = Path.Combine(folder, $"carillon_before_restore_{stamp}_{Guid.NewGuid():N}.zip");
        var legacyExecutable = Path.Combine(plan.InstallationFolder, "Carillon.exe");
        var result = new Restore724Result(database, savedBackup, File.Exists(legacyExecutable) ? legacyExecutable : null);
        try
        {
            progress?.Report("Checking the 7.2.4 backup ZIP...");
            var backup = StageRestoreDatabase(plan.BackupPath, candidate, cancellationToken);
            if (!backup.Hash.SequenceEqual(plan.BackupFingerprint))
                throw new IOException("The 7.2.4 backup changed after review. Select and review it again.");

            progress?.Report("Backing up the current v9 database...");
            using (var guard = ProtectDatabase(database))
            {
                if (!CopyOrHash(guard, null, cancellationToken).SequenceEqual(plan.CurrentFingerprint))
                    throw new IOException("The database changed after review. Select and review the USB again.");
                WriteBackup(database, temporaryBackup, cancellationToken);
            }

            File.Move(temporaryBackup, savedBackup);
            progress?.Report("Restoring the 7.2.4 database...");
            using (var guard = ProtectDatabase(database))
            {
                if (!CopyOrHash(guard, null, cancellationToken).SequenceEqual(plan.CurrentFingerprint))
                    throw new IOException("The database changed after backup. Select and review the USB again.");
            }

            CheckPath(stage);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(candidate, database, overwrite: true);
            return result;
        }
        finally
        {
            File.Delete(candidate);
            File.Delete(temporaryBackup);
            Directory.Delete(stage);
        }
    }

    private static string ResolveRestoreDatabase(string folder)
    {
        if (!Path.IsPathFullyQualified(folder))
            throw new IOException("Select the USB or installation folder that contains the databases folder.");
        var database = Path.Combine(FullPath(folder), UsbDatabaseEntry);
        CheckPath(database);
        if (!File.Exists(database))
            throw new FileNotFoundException("The selected folder does not contain databases\\carillon.db.", database);
        return database;
    }

    private static void CheckRestoreTarget(SqliteConnection current)
    {
        if (InspectDatabase(current).Version == LegacyVersion.Version724)
            throw new InvalidDataException("This installation already has a 7.2.4 database.");
        using var command = current.CreateCommand();
        command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND " +
            "(upper(name)='CARILLON9_DISPATCHED' OR upper(name) GLOB 'PL_RAND_DIR_ROTATION_WIDE_BACKUP*')";
        if (Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) == 0)
            throw new InvalidDataException("The selected database does not show an upgrade to v9. Select the upgraded installation.");
    }

    private static string CreateRestoreStage(string folder)
    {
        var stage = Path.Combine(folder, $".carillon-restore-{Guid.NewGuid():N}");
        CheckPath(stage);
        if (!CreateStageDirectory(stage, IntPtr.Zero))
            throw new IOException("The temporary restore folder could not be created.",
                new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
        return stage;
    }

    private static (DateTime Date, int Count, byte[] Hash) StageRestoreDatabase(string archive, string candidate,
        CancellationToken cancellationToken)
    {
        CheckPath(archive);
        using var zip = ZipFile.OpenRead(archive);
        var entries = zip.Entries.Where(entry => SamePath(entry.FullName, "carillon.db") ||
            SamePath(entry.FullName.Replace('\\', '/'), ZipEntryName(UsbDatabaseEntry))).ToArray();
        if (entries.Length != 1)
            throw new InvalidDataException("The ZIP must contain one carillon.db at its top level or in its databases folder.");

        var original = entries[0];
        byte[] hash;
        using (var contents = original.Open())
        using (var output = new FileStream(candidate, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            hash = CopyOrHash(contents, output, cancellationToken);
            if (output.Length != original.Length)
                throw new InvalidDataException("The ZIP contains an incomplete database.");
            output.Flush(flushToDisk: true);
        }

        using var database = OpenDatabase(candidate, SqliteOpenMode.ReadOnly, immutable: true);
        var inspected = InspectDatabase(database);
        if (inspected.Version != LegacyVersion.Version724)
            throw new InvalidDataException("The ZIP does not contain the original 7.2.4 database.");
        cancellationToken.ThrowIfCancellationRequested();
        return (original.LastWriteTime.LocalDateTime, inspected.Tracks.Count, hash);
    }
}
