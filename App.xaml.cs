using Microsoft.Data.Sqlite;
using System.IO;
using System.Windows;
using Carillon.Migration;
namespace Carillon;
public partial class App : Application
{
    internal const string ProductName = "Westminster Chimes and Carillon Bells";
    internal const string ProductTitle = ProductName + " v9.0";

    public App()
    {
        var culture=new System.Globalization.CultureInfo("en-US",false);
        culture.DateTimeFormat.ShortDatePattern="MM/dd/yyyy";
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture=culture;
        System.Globalization.CultureInfo.CurrentCulture=culture;
    }
    private FileStream? instanceLock,databaseLock;
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if(e.Args.Contains("--self-test")) { ShutdownMode=ShutdownMode.OnExplicitShutdown; var fontDataIndex=Array.IndexOf(e.Args,"--font-database");var fontData=fontDataIndex>=0&&fontDataIndex+1<e.Args.Length?e.Args[fontDataIndex+1]:null;var code=RegressionTests.Run(AppContext.BaseDirectory,fontData); Shutdown(code); return; }
        if(e.Args.Length==2&&e.Args[0]=="--audio-test") { ShutdownMode=ShutdownMode.OnExplicitShutdown;var code=await AudioRegressionTests.Run(AppContext.BaseDirectory,e.Args[1]);Shutdown(code);return; }
        try
        {
            var root=AppContext.BaseDirectory;
            var database=Path.Combine(root,"databases","carillon.db");
#if CARILLON_SINGLE_EXE
            if(!File.Exists(database))
                throw new FileNotFoundException("The USB database was not found at:\n"+database+"\n\nOpen CarillonBells.exe from the USB that contains the starter or existing database.");
#endif
            instanceLock=new FileStream(Path.Combine(root,"Carillon.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            Directory.CreateDirectory(Path.GetDirectoryName(database)!);
            databaseLock=new FileStream(database+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
#if !CARILLON_SINGLE_EXE
            if(!File.Exists(database))
            {
                if(MessageBox.Show("This installation has no database. Create a blank SQLite database? You can import a copy of your existing database from Database import.",ProductTitle+" - First start",MessageBoxButton.YesNo)!=MessageBoxResult.Yes) { Shutdown(); return; }
                using var connection=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=database,Pooling=false}.ToString()); connection.Open(); using var command=connection.CreateCommand();
                using var stream=GetResourceStream(new Uri("pack://application:,,,/samples/empty-schema.sql"))?.Stream??throw new FileNotFoundException("The empty database schema is missing."); using var reader=new StreamReader(stream); command.CommandText=reader.ReadToEnd(); command.ExecuteNonQuery();
            }
#endif
            var upgradeRequired=MigrationEngine.LocalDatabaseRequiresUpgrade(database);
            if(upgradeRequired&&MessageBox.Show(
                "A 7.2.4 database was found. Close the older application before continuing.\n\nA backup ZIP will be saved in the databases folder before the database is upgraded. Continue?",
                ProductTitle+" - Upgrade from 7.2.4",MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.No)!=MessageBoxResult.Yes)
            {
                Shutdown(); return;
            }

            string? upgradeBackup=null;
            if(upgradeRequired) upgradeBackup=await Task.Run(()=>MigrationEngine.UpgradeLocalDatabase(database));

            var repository=new CarillonRepository(database); repository.EnsureCompatibleSchema(backupInDatabaseFolder:true); ThemeManager.Load(repository);
            var splash=new SplashWindow(); splash.Show();
            await Task.Delay(900);
            var migrationReview=upgradeRequired||e.Args.Contains("--migration-review");
            var runtime=new CarillonRuntime(repository,root,Dispatcher,e.Args.Contains("--evaluation"),migrationReview:migrationReview);
            if(upgradeBackup is not null) runtime.Log.Append("7.2.4 database upgraded. Backup ZIP: "+upgradeBackup);
            var handStyles = (ClockHandStyle[])FindResource("ClockHandCatalog");
            Resources["SelectedClockHands"] = handStyles.FirstOrDefault(style => style.Id == runtime.ClockHandStyleId) ?? handStyles[0];
            var window=new MainWindow(runtime); MainWindow=window; window.Show(); splash.Close();
            if(ThemeManager.LoadFonts(runtime) is {} fontNotice) MessageBox.Show(window,fontNotice,"Font settings",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        catch(Exception error) { MessageBox.Show("The application could not start: "+error.Message+"\n\nCheck that the portable folder is writable and another instance is not using it.",ProductTitle+" - Startup"); Shutdown(1); }
    }
    protected override void OnExit(ExitEventArgs e) { databaseLock?.Dispose(); instanceLock?.Dispose(); base.OnExit(e); }
}
