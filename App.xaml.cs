using Microsoft.Data.Sqlite;
using System.IO;
using System.Windows;
namespace Carillon;
public partial class App : Application
{
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
        if(e.Args.Contains("--self-test")) { var code=RegressionTests.Run(AppContext.BaseDirectory); Shutdown(code); return; }
        if(e.Args.Length==2&&e.Args[0]=="--audio-test") { ShutdownMode=ShutdownMode.OnExplicitShutdown;var code=await AudioRegressionTests.Run(AppContext.BaseDirectory,e.Args[1]);Shutdown(code);return; }
        try
        {
            var root=AppContext.BaseDirectory;
            instanceLock=new FileStream(Path.Combine(root,"Carillon.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            var database=Path.Combine(root,"databases","carillon.db"); Directory.CreateDirectory(Path.GetDirectoryName(database)!);
            databaseLock=new FileStream(database+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            if(!File.Exists(database))
            {
                if(MessageBox.Show("This separate Carillon installation has no database. Create a blank SQLite database? You can import a copy of your existing database from Database import.","Carillon first start",MessageBoxButton.YesNo)!=MessageBoxResult.Yes) { Shutdown(); return; }
                using var connection=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=database,Pooling=false}.ToString()); connection.Open(); using var command=connection.CreateCommand();
                using var stream=GetResourceStream(new Uri("pack://application:,,,/samples/empty-schema.sql"))?.Stream??throw new FileNotFoundException("The empty database schema is missing."); using var reader=new StreamReader(stream); command.CommandText=reader.ReadToEnd(); command.ExecuteNonQuery();
            }
            var repository=new CarillonRepository(database); repository.EnsureCompatibleSchema(); ThemeManager.Load(repository);
            var splash=new SplashWindow(); splash.Show();
            await Task.Delay(900);
            var runtime=new CarillonRuntime(repository,root,Dispatcher,e.Args.Contains("--evaluation"));
            var handStyles = (ClockHandStyle[])FindResource("ClockHandCatalog");
            Resources["SelectedClockHands"] = handStyles.FirstOrDefault(style => style.Id == runtime.ClockHandStyleId) ?? handStyles[0];
            var window=new MainWindow(runtime); MainWindow=window; window.Show(); splash.Close();
        }
        catch(Exception error) { MessageBox.Show("Carillon could not start: "+error.Message+"\n\nCheck that the portable folder is writable and another instance is not using it.","Carillon startup"); Shutdown(1); }
    }
    protected override void OnExit(ExitEventArgs e) { databaseLock?.Dispose(); instanceLock?.Dispose(); base.OnExit(e); }
}
