using System.Windows;
namespace Carillon;
public partial class LogWindow : Window
{
    private readonly CarillonLog log;
    public LogWindow(CarillonLog log,bool appearancePreview=false) { this.log=log; InitializeComponent(); if(appearancePreview) {Entries.EnableRowVirtualization=false;Entries.EnableColumnVirtualization=false;Entries.ItemsSource=log.Read().TakeLast(1000).Reverse().ToList();} else RefreshClick(this,new RoutedEventArgs()); }
    private async void RefreshClick(object sender,RoutedEventArgs e) { try { Entries.ItemsSource=await Task.Run(()=>log.Read().TakeLast(1000).Reverse().ToList()); } catch(Exception error) { MessageBox.Show(this,error.Message,"Log read failed"); } }
    private void CloseClick(object sender,RoutedEventArgs e)=>Close();
}
