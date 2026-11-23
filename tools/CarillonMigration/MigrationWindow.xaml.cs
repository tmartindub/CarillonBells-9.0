using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Carillon.Migration;

public partial class MigrationWindow : Window, INotifyPropertyChanged
{
    private enum WizardStep { Source, Destination, Review, Result }

    private readonly string templateFolder = AppContext.BaseDirectory;
    private readonly string? singleFileExecutable;
    private readonly string defaultDestinationName;
    private MigrationSource? source;
    private MigrationPlan? plan;
    private UsbUpgradePlan? usbUpgradePlan;
    private MigrationResult? result;
    private bool busy;
    private CancellationTokenSource? migrationCancellation;

    public int CurrentStep { get; private set; }
    public bool IsUsbUpgrade => singleFileExecutable is not null;
    public bool CanNavigate => !busy;
    public bool IsMigrating => migrationCancellation is not null;
    public bool CanCancelMigration => migrationCancellation is { IsCancellationRequested: false };

    public event PropertyChangedEventHandler? PropertyChanged;

    public MigrationWindow() : this(null) { }

    public MigrationWindow(string? singleFileExecutable = null)
    {
        this.singleFileExecutable = singleFileExecutable;
        InitializeComponent();
        defaultDestinationName = DestinationInput.Text;
        if (IsUsbUpgrade)
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.DoNotVerify);
            if (!string.IsNullOrEmpty(localAppData))
                BackupFolderInput.Text = Path.Combine(localAppData, "Carillon Backups");
        }
        DataContext = this;
    }

    private void BrowseSourceClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog { Title = "Choose the legacy Carillon installation folder" };
            var current = SourceInput.Text.Trim().Trim('"');
            if (Directory.Exists(current))
                dialog.InitialDirectory = current;
            if (dialog.ShowDialog(this) == true)
            {
                SourceInput.Text = dialog.FolderName;
                StatusText.Text = "";
            }
        }
        catch (Exception error) { StatusText.Text = error.Message; }
    }

    private void BrowseDestinationClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var entered = DestinationInput.Text.Trim().Trim('"');
            var folderName = Path.GetFileName(Path.TrimEndingDirectorySeparator(entered));
            if (string.IsNullOrWhiteSpace(folderName))
                folderName = defaultDestinationName;

            var dialog = new OpenFolderDialog { Title = "Choose the parent for the new v9 folder" };
            if (Path.IsPathFullyQualified(entered) && Path.GetDirectoryName(entered) is { } parent && Directory.Exists(parent))
                dialog.InitialDirectory = parent;
            if (dialog.ShowDialog(this) == true)
            {
                DestinationInput.Text = Path.Combine(dialog.FolderName, folderName);
                StatusText.Text = "";
            }
        }
        catch (Exception error) { StatusText.Text = error.Message; }
    }

    private void BrowseBackupClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var entered = BackupFolderInput.Text.Trim().Trim('"');
            var dialog = new OpenFolderDialog { Title = "Choose a folder for the pre-upgrade ZIP backup" };
            if (Directory.Exists(entered))
                dialog.InitialDirectory = entered;
            else if (Path.IsPathFullyQualified(entered) && Path.GetDirectoryName(entered) is { } parent && Directory.Exists(parent))
                dialog.InitialDirectory = parent;
            if (dialog.ShowDialog(this) == true)
            {
                BackupFolderInput.Text = dialog.FolderName;
                StatusText.Text = "";
            }
        }
        catch (Exception error) { StatusText.Text = error.Message; }
    }

    private async void AdvanceClick(object sender, RoutedEventArgs e)
    {
        if (!CanNavigate)
            return;

        StatusText.Text = "";
        busy = true;
        NotifyWorkState();
        try
        {
            switch ((WizardStep)CurrentStep)
            {
                case WizardStep.Source:
                    await InspectSourceAsync();
                    break;
                case WizardStep.Destination:
                    await ReviewPlanAsync();
                    break;
                case WizardStep.Review when ReviewConfirmed.IsChecked == true:
                    await RunMigrationAsync();
                    break;
            }
        }
        catch (Exception error) { StatusText.Text = error.Message; }
        finally
        {
            busy = false;
            NotifyWorkState();
        }

        if (CurrentStep == (int)WizardStep.Destination)
            (IsUsbUpgrade ? BackupFolderInput : DestinationInput).Focus();
    }

    private async Task InspectSourceAsync()
    {
        source = null;
        plan = null;
        usbUpgradePlan = null;
        var folder = SourceInput.Text.Trim().Trim('"');
        if (!Path.IsPathFullyQualified(folder))
            throw new ArgumentException("Enter the full path of the legacy installation folder or choose Browse.");

        StatusText.Text = "Inspecting the legacy installation…";
        var inspected = await Task.Run(() => MigrationEngine.InspectSource(folder));
        source = inspected;
        var version = inspected.Version == LegacyVersion.Version724 ? "7.2.4" : "8.1";
        InspectedSource.Text = $"Detected Carillon {version} · {inspected.PlaylistCount:N0} playlist entries";
        ShowStep(WizardStep.Destination);
    }

    private async Task ReviewPlanAsync()
    {
        var inspected = source ?? throw new InvalidOperationException("Go back and inspect the legacy installation first.");
        IReadOnlyList<string> warnings;
        ReviewDatabase.Text = inspected.DatabasePath;
        if (singleFileExecutable is { } executable)
        {
            var backupFolder = BackupFolderInput.Text.Trim().Trim('"');
            if (!Path.IsPathFullyQualified(backupFolder))
                throw new ArgumentException("Enter the full path of the backup folder or choose Browse.");

            StatusText.Text = "Checking the USB installation and backup folder…";
            var prepared = await Task.Run(() => MigrationEngine.PlanUsbUpgrade(inspected, backupFolder, executable));
            usbUpgradePlan = prepared;
            ReviewSummary.Text = InspectedSource.Text;
            ReviewRoute.Text = inspected.Version == LegacyVersion.Version724
                ? "7.2.4 → 8.1 database schema → Carillon 9.0"
                : "8.1 → Carillon 9.0 · The 8.1 database schema is retained.";
            ReviewDestination.Text = prepared.Source.Folder;
            ReviewBackupFolder.Text = prepared.BackupFolder;
            ReviewMusic.Text = "Music stays in its current folders on this USB.";
            warnings = prepared.Warnings;
        }
        else
        {
            var destination = DestinationInput.Text.Trim().Trim('"');
            if (!Path.IsPathFullyQualified(destination))
                throw new ArgumentException("Enter a full path for the new folder or choose Browse parent.");

            StatusText.Text = "Checking the destination and counting music files…";
            var prepared = await Task.Run(() => MigrationEngine.Plan(inspected, destination, templateFolder));
            plan = prepared;
            var conversion = inspected.Version == LegacyVersion.Version724
                ? "The database copy will be upgraded."
                : "The database schema will be preserved.";
            ReviewSummary.Text = $"{InspectedSource.Text} · {conversion}";
            ReviewDestination.Text = prepared.Destination;
            ReviewMusic.Text = $"{prepared.MusicFileCount:N0} music files · {prepared.MusicBytes:N0} bytes";
            warnings = prepared.Warnings;
        }
        ReviewWarningTitle.Text = warnings.Count == 0 ? "No warnings" : "Warnings to review";
        ReviewWarnings.ItemsSource = warnings;
        ReviewConfirmed.IsChecked = false;
        ShowStep(WizardStep.Review);
    }

    private async Task RunMigrationAsync()
    {
        using var cancellation = new CancellationTokenSource();
        migrationCancellation = cancellation;
        NotifyWorkState();
        StatusText.Text = IsUsbUpgrade ? "Starting the USB upgrade…" : "Starting migration…";
        var progress = new Progress<string>(message =>
        {
            if (ReferenceEquals(migrationCancellation, cancellation) && !cancellation.IsCancellationRequested)
                StatusText.Text = message;
        });

        try
        {
            MigrationResult completed;
            if (singleFileExecutable is { } executable)
            {
                var prepared = usbUpgradePlan ?? throw new InvalidOperationException("Review the USB upgrade before starting.");
                completed = await Task.Run(() => MigrationEngine.UpgradeUsb(prepared, executable, progress, cancellation.Token));
            }
            else
            {
                var prepared = plan ?? throw new InvalidOperationException("Review the migration before starting.");
                completed = await Task.Run(() => MigrationEngine.Run(prepared, templateFolder, progress, cancellation.Token));
            }
            result = completed;
            ResultSummary.Text = IsUsbUpgrade
                ? "Carillon 9.0 is installed on the selected USB. Music remains in its current folders."
                : $"Created the v9 installation and copied {completed.MusicFilesCopied:N0} music files.";
            ResultDestination.Text = completed.Destination;
            ResultBackup.Text = completed.BackupPath;
            ResultReport.Text = completed.ReportPath;
            ResultWarningTitle.Text = completed.Warnings.Count == 0 ? "No warnings" : "Review these warnings";
            ResultWarnings.ItemsSource = completed.Warnings;
            ShowStep(WizardStep.Result);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            StatusText.Text = IsUsbUpgrade
                ? "USB upgrade canceled. Review the settings before starting again."
                : "Migration canceled. Review the settings before starting again.";
            ReviewConfirmed.IsChecked = false;
        }
        catch
        {
            ReviewConfirmed.IsChecked = false;
            throw;
        }
        finally
        {
            migrationCancellation = null;
            NotifyWorkState();
        }
    }

    private void BackClick(object sender, RoutedEventArgs e)
    {
        if (!CanNavigate)
            return;

        switch ((WizardStep)CurrentStep)
        {
            case WizardStep.Destination:
                source = null;
                ShowStep(WizardStep.Source);
                SourceInput.Focus();
                break;
            case WizardStep.Review:
                plan = null;
                usbUpgradePlan = null;
                ReviewConfirmed.IsChecked = false;
                ShowStep(WizardStep.Destination);
                (IsUsbUpgrade ? BackupFolderInput : DestinationInput).Focus();
                break;
        }
    }

    private void CancelMigrationClick(object sender, RoutedEventArgs e)
    {
        if (migrationCancellation is not { IsCancellationRequested: false } cancellation)
            return;
        StatusText.Text = IsUsbUpgrade
            ? "Canceling upgrade and cleaning up temporary files…"
            : "Canceling migration and cleaning up temporary files…";
        cancellation.Cancel();
        NotifyWorkState();
    }

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (CanNavigate)
            return;

        e.Cancel = true;
        StatusText.Text = IsMigrating
            ? "Wait for the operation to finish or cancel it and wait for cleanup."
            : "Wait for the current check to finish before closing.";
    }

    private void CloseClick(object sender, RoutedEventArgs e) => Close();

    private void OpenDestinationClick(object sender, RoutedEventArgs e)
    {
        if (result is not { } completed)
            return;
        try
        {
            using var process = Process.Start(new ProcessStartInfo(completed.Destination) { UseShellExecute = true });
        }
        catch (Exception error) { StatusText.Text = error.Message; }
    }

    private void OpenReviewClick(object sender, RoutedEventArgs e)
    {
        if (result is not { } completed)
            return;
        try
        {
            var executable = Path.Combine(completed.Destination, "CarillonBells.exe");
            if (!File.Exists(executable))
                throw new FileNotFoundException("CarillonBells.exe is missing from the installation.", executable);
            using var process = Process.Start(new ProcessStartInfo(executable, "--migration-review")
            {
                WorkingDirectory = completed.Destination,
                UseShellExecute = true
            });
        }
        catch (Exception error) { StatusText.Text = error.Message; }
    }

    private void ShowStep(WizardStep step)
    {
        CurrentStep = (int)step;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStep)));
        StatusText.Text = "";
        PageScroll.ScrollToTop();
    }

    private void NotifyWorkState()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanNavigate)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMigrating)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCancelMigration)));
    }
}
