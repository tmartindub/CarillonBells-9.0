using System.Windows;
using System.Windows.Threading;
namespace Carillon;
public partial class ClockWindow : Window
{
    private readonly DispatcherTimer timer;
    public ClockWindow() { InitializeComponent(); timer=new DispatcherTimer(TimeSpan.FromSeconds(1),DispatcherPriority.Background,(_,_)=>UpdateClock(),Dispatcher); UpdateClock(); }
    private void UpdateClock() { var now=DateTime.Now; MinuteRotation.Angle=now.Minute*6; HourRotation.Angle=(now.Hour%12)*30+now.Minute*0.5; }
    private void CloseClick(object sender,RoutedEventArgs e)=>Close();
    private void ClockClosed(object? sender,EventArgs e)=>timer.Stop();
}
