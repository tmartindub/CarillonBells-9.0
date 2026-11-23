using System.Windows;

namespace Carillon.Migration;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow = new MigrationWindow();
        MainWindow.Show();
    }
}
