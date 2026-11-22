using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Carillon;

public partial class MainWindow : Window,INotifyPropertyChanged
{
    private readonly CarillonRuntime runtime;
    private Song? selectedSong,editingSong;
    private string originalFingerprint="";
    private bool editSession,resumeSchedule,selecting,volumeUpdating;
    private ClockWindow? clock;
    public ObservableCollection<Song> Songs { get; } = [];
    public ObservableCollection<string> ScheduleRows { get; } = [];
    public Song? SelectedSong
    {
        get=>selectedSong;
        set
        {
            if(selecting||ReferenceEquals(value,selectedSong)) return;
            if(!ConfirmEdits()) { Dispatcher.BeginInvoke(()=>{ selecting=true; SongGrid.SelectedItem=selectedSong; selecting=false; }); return; }
            selectedSong=value; editingSong=value?.Clone(); originalFingerprint=editingSong?.Fingerprint??""; Notify(); Notify(nameof(EditingSong));
        }
    }
    public Song? EditingSong=>editingSong;
    public bool ScheduleVisible { get; private set; }
    public string Organization=>runtime.Setting("ORGANIZATION_NAME");
    public string TodayLabel=>$"{Organization}  ·  {DateTime.Now:dddd, MMMM d, yyyy  h:mm:ss tt}";
    public string NextBellLabel=>runtime.ScheduleEnabled?runtime.Queue.FirstOrDefault()?.ToString()??"No remaining bells":"Schedule paused";
    public string ScheduleCountLabel=>$"{runtime.FullDay(DateTime.Now).Count} today / {runtime.Queue.Count} queued";
    public string SilenceLabel=>runtime.SilenceLabel;
    public string PlayerTitle=>runtime.Player.Busy?runtime.Player.CurrentTitle:SelectedSong?.Title??"Select a song";
    public string PlaybackLabel=>runtime.Player.Busy?$"{(runtime.Player.Paused?"Paused":"Playing")} · {DataFormats.DurationText(runtime.Player.Duration-runtime.Player.Position)} remaining":runtime.LastPlayed;
    public string LibraryLabel=>$"{Songs.Count} songs · {DataFormats.DurationText(TimeSpan.FromTicks(Songs.Sum(s=>DataFormats.Duration(s.Duration).Ticks)))}";
    public string Footer=>$"9.0.0 · {(runtime.Evaluation?"Evaluation":"Portable")} · Uptime {DataFormats.DurationText(DateTime.Now-runtime.Started)}";
    public string Status=>runtime.Error.Length>0?runtime.Error:$"Recorded plays today: {runtime.PlayedToday} · {runtime.Audio.DeviceName}";
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([System.Runtime.CompilerServices.CallerMemberName]string? name=null)=>PropertyChanged?.Invoke(this,new(name));
    public MainWindow(CarillonRuntime runtime)
    {
        this.runtime=runtime; InitializeComponent(); DataContext=this; LoadSongs(); runtime.Changed+=UpdateStatus;
        runtime.Audio.Changed+=UpdateVolume; UpdateVolume(); UpdateStatus();
    }
    private void UpdateStatus()
    {
        if(!Dispatcher.CheckAccess()) { Dispatcher.BeginInvoke(UpdateStatus); return; }
        foreach(var property in new[]{nameof(TodayLabel),nameof(NextBellLabel),nameof(ScheduleCountLabel),nameof(SilenceLabel),nameof(PlayerTitle),nameof(PlaybackLabel),nameof(Footer),nameof(Status),nameof(Organization)}) Notify(property);
        ScheduleToggle.IsChecked=runtime.ScheduleEnabled;
        PlayerProgress.Maximum=Math.Max(1,runtime.Player.Duration.TotalSeconds); PlayerProgress.Value=runtime.Player.Position.TotalSeconds;
    }
    private void LoadSongs(string? key=null)
    {
        key??=selectedSong?.Key; var expansion=Descendants<Expander>(SongGrid).Where(x=>x.DataContext is CollectionViewGroup).ToDictionary(x=>((CollectionViewGroup)x.DataContext).Name.ToString()!,x=>x.IsExpanded);
        var scroll=Descendants<ScrollViewer>(SongGrid).FirstOrDefault(); var offset=scroll?.VerticalOffset??0;
        selecting=true; Songs.Clear(); foreach(var song in runtime.Songs) Songs.Add(song); selecting=false;
        selectedSong=null; SelectedSong=Songs.FirstOrDefault(s=>s.Key==key)??Songs.FirstOrDefault(); Notify(nameof(LibraryLabel));
        Dispatcher.BeginInvoke(()=>{ foreach(var group in Descendants<Expander>(SongGrid)) if(group.DataContext is CollectionViewGroup data&&expansion.TryGetValue(data.Name.ToString()!,out var open)) group.IsExpanded=open; scroll?.ScrollToVerticalOffset(offset); });
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T:DependencyObject
    {
        for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++) { var child=VisualTreeHelper.GetChild(root,i); if(child is T match) yield return match; foreach(var nested in Descendants<T>(child)) yield return nested; }
    }
    private bool Dirty=>EditingSong is not null&&EditingSong.Fingerprint!=originalFingerprint;
    private void BeginEdit(object sender,KeyboardFocusChangedEventArgs e)
    {
        if(editSession||EditingSong is null) return; editSession=true; resumeSchedule=runtime.ScheduleEnabled;
        if(resumeSchedule) runtime.SetSchedule(false);
    }
    private void EndEdit()
    {
        var resume=editSession&&resumeSchedule; editSession=false; resumeSchedule=false; if(resume) runtime.SetSchedule(true);
    }
    private bool Save()
    {
        if(EditingSong is null||SelectedSong is null) { EndEdit(); return true; }
        try { foreach(var box in Descendants<TextBox>(this)) { var binding=box.GetBindingExpression(TextBox.TextProperty);binding?.UpdateSource();if(Validation.GetHasError(box))throw new ArgumentException("Correct the invalid field before saving."); } EditingSong.Validate(); runtime.Repository.SaveSong(EditingSong,SelectedSong); originalFingerprint=EditingSong.Fingerprint; var key=EditingSong.Key; EndEdit(); runtime.Reload(); if(runtime.ScheduleEnabled) runtime.Rebuild(DateTime.Now); LoadSongs(key); return true; }
        catch(Exception error) { MessageBox.Show(this,error.Message,"Cannot save song"); return false; }
    }
    private bool ConfirmEdits()
    {
        if(!Dirty) { EndEdit(); return true; }
        var answer=MessageBox.Show(this,"Save changes to this song?","Pending changes",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
        if(answer==MessageBoxResult.Cancel) return false; if(answer==MessageBoxResult.Yes) return Save();
        editingSong=selectedSong?.Clone(); originalFingerprint=editingSong?.Fingerprint??""; EndEdit(); return true;
    }
    private void SaveClick(object sender,RoutedEventArgs e)=>Save();
    private void CancelClick(object sender,RoutedEventArgs e) { editingSong=selectedSong?.Clone(); originalFingerprint=editingSong?.Fingerprint??""; Notify(nameof(EditingSong)); EndEdit(); }
    private void ToggleSchedule(object sender,RoutedEventArgs e)
    {
        if(!ConfirmEdits()) { UpdateStatus(); return; }
        try { runtime.SetSchedule(ScheduleToggle.IsChecked==true); LoadSongs(); } catch(Exception error) { ShowError(error); }
    }
    private void SearchChanged(object sender,TextChangedEventArgs e)
    {
        if(SongGrid is null) return;
        var text=SearchBox.Text; var view=CollectionViewSource.GetDefaultView(SongGrid.ItemsSource);
        if(view is not null) view.Filter=item=>item is Song song&&(song.Title.Contains(text,StringComparison.OrdinalIgnoreCase)||song.Group.Contains(text,StringComparison.OrdinalIgnoreCase));
    }
    private void ExpandClick(object sender,RoutedEventArgs e) { foreach(var expander in Descendants<Expander>(SongGrid)) expander.IsExpanded=true; }
    private void CollapseClick(object sender,RoutedEventArgs e) { foreach(var expander in Descendants<Expander>(SongGrid)) expander.IsExpanded=false; }
    private void NavigateClick(object sender,RoutedEventArgs e)
    {
        var visible=SongGrid.Items.OfType<Song>().ToList(); if(visible.Count==0) return; var index=Math.Max(0,visible.IndexOf(SelectedSong!));
        index=((Button)sender).Tag?.ToString() switch {"First"=>0,"Last"=>visible.Count-1,"Prior"=>Math.Max(0,index-1),_=>Math.Min(visible.Count-1,index+1)};
        SelectedSong=visible[index]; SongGrid.ScrollIntoView(SelectedSong);
    }
    private async void AddClick(object sender,RoutedEventArgs e)
    {
        if(!ConfirmEdits()) return; var dialog=new OpenFileDialog {Filter="MP3 / WAV audio|*.mp3;*.wav",Multiselect=true,Title="Add music files"}; if(dialog.ShowDialog(this)!=true) return;
        var enabled=runtime.ScheduleEnabled; runtime.SetSchedule(false);
        try { foreach(var path in dialog.FileNames) { var duration=await Task.Run(()=>AudioPlayback.ReadDuration(path)); var relative=Path.GetRelativePath(runtime.BaseDirectory,path); var stored=relative.StartsWith("..")?path:".\\"+relative; var song=new Song {Path=stored,Duration=DataFormats.DurationText(duration),From=DateTime.Today,To=DateTime.Today.AddYears(1)}; runtime.Repository.SaveSong(song); } }
        catch(Exception error) { ShowError(error); }
        finally { runtime.Reload(); if(enabled) runtime.SetSchedule(true); LoadSongs(); }
    }
    private void DeleteClick(object sender,RoutedEventArgs e)
    {
        if(!ConfirmEdits()||SelectedSong is null) return;
        if(MessageBox.Show(this,"Delete "+SelectedSong.Title+" from the playlist? The audio file is kept.","Delete song",MessageBoxButton.YesNo)!=MessageBoxResult.Yes) return;
        var index=Songs.IndexOf(SelectedSong);
        try { runtime.Repository.DeleteSong(SelectedSong); runtime.Reload(); if(runtime.ScheduleEnabled) runtime.Rebuild(DateTime.Now); LoadSongs(runtime.Songs.ElementAtOrDefault(Math.Min(index,runtime.Songs.Count-1))?.Key); } catch(Exception error) { ShowError(error); }
    }
    private void RefreshClick(object sender,RoutedEventArgs e) { if(!ConfirmEdits()) return; try { runtime.Reload(); LoadSongs(); } catch(Exception error) { ShowError(error); } }
    private void PlayClick(object sender,RoutedEventArgs e) { if(SelectedSong is null) return; try { runtime.Play(SelectedSong); } catch(Exception error) { ShowError(error); } }
    private void PauseClick(object sender,RoutedEventArgs e)=>runtime.Player.PauseResume();
    private void StopClick(object sender,RoutedEventArgs e)=>runtime.Player.Stop();
    private void UpdateVolume() { volumeUpdating=true; VolumeSlider.Value=runtime.Audio.Volume*100; VolumeSlider.IsEnabled=!runtime.Audio.Muted; MuteButton.Content=runtime.Audio.Muted?"Unmute":"Mute"; volumeUpdating=false; }
    private void VolumeChanged(object sender,RoutedPropertyChangedEventArgs<double> e) { if(!volumeUpdating&&runtime is not null) runtime.Audio.Volume=(float)(e.NewValue/100); }
    private void MuteClick(object sender,RoutedEventArgs e)=>runtime.Audio.ToggleManualMute();
    private void ScheduleClick(object sender,RoutedEventArgs e)
    {
        ScheduleVisible=!ScheduleVisible; ScheduleRows.Clear();
        if(ScheduleVisible) { ScheduleRows.Add("FULL DAY"); foreach(var bell in runtime.FullDay(DateTime.Now)) ScheduleRows.Add(bell.ToString()); ScheduleRows.Add("REMAINING QUEUE"); foreach(var bell in runtime.Queue.Where(b=>b.When>=DateTime.Now)) ScheduleRows.Add(bell.ToString()); foreach(var rule in runtime.Rules) foreach(var day in new[]{DateTime.Today.AddDays(-1),DateTime.Today}) if(rule.Occurrence(day) is {} occurrence&&occurrence.End>DateTime.Now) ScheduleRows.Add($"Silence {occurrence.Start:g} – {occurrence.End:g} / {rule.Purpose}"); }
        Notify(nameof(ScheduleVisible));
    }
    private void AdminClick(object sender,RoutedEventArgs e)
    {
        if(!ConfirmEdits()) return; var page=((Button)sender).Tag?.ToString()??"System"; var pauses=page is not "Themes";
        if(pauses) runtime.SetSchedule(false);
        AdministrationWindow? administration=null;
        try { administration=new AdministrationWindow(runtime,page){Owner=this};administration.ShowDialog(); runtime.Reload(); LoadSongs(); }
        catch(Exception error) { ShowError(error); }
        finally { if(pauses||administration?.RequiresScheduleResume==true) {runtime.SetSchedule(true);LoadSongs();} }
    }
    private void ClockClick(object sender,RoutedEventArgs e) { if(clock is null) { clock=new ClockWindow(); clock.Closed+=(_,_)=>clock=null; clock.Show(); } else clock.Activate(); }
    private void LogClick(object sender,RoutedEventArgs e)=>new LogWindow(runtime.Log){Owner=this}.Show();
    private void HelpClick(object sender,RoutedEventArgs e) { try { Process.Start(new ProcessStartInfo(Path.Combine(runtime.BaseDirectory,"help","index.html")){UseShellExecute=true}); } catch(Exception error) { ShowError(error); } }
    private void ExitClick(object sender,RoutedEventArgs e)=>Close();
    private void ShowError(Exception error) { runtime.Fail(error.Message); MessageBox.Show(this,error.Message,"Carillon"); }
    private void WindowClosing(object? sender,CancelEventArgs e) { if(!ConfirmEdits()) { e.Cancel=true; return; } clock?.Close(); runtime.Changed-=UpdateStatus; runtime.Audio.Changed-=UpdateVolume; runtime.Dispose(); }
}
