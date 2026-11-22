using Microsoft.Data.Sqlite;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Carillon;
public static class AudioRegressionTests
{
    public static async Task<int> Run(string baseDirectory,string mp3Path)
    {
        var results=new List<string>();var scratch=Path.Combine(baseDirectory,"audio-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(scratch);
        void Check(string name,bool condition) {if(!condition)throw new Exception(name);results.Add("PASS "+name);}
        ClockWindow? clock=null; CarillonRuntime? runtime=null;
        try
        {
            var wavePath=Path.Combine(scratch,"validation.wav");
            using(var writer=new WaveFileWriter(wavePath,new WaveFormat(22050,16,1))) for(int i=0;i<22050;i++) writer.WriteSample((float)(0.015*Math.Sin(2*Math.PI*440*i/22050)));
            foreach(var path in new[]{mp3Path,wavePath})
            {
                for(int i=0;i<20;i++) {using var reader=AudioPlayback.OpenFile(path);Check($"{Path.GetExtension(path)} duration cycle {i+1}",reader.TotalTime>TimeSpan.Zero);var buffer=new byte[16384];int read=0,count;while((count=reader.Read(buffer,0,buffer.Length))>0)read+=count;Check($"{Path.GetExtension(path)} decoding cycle {i+1}",read>0);}
            }
            using(var player=new AudioPlayback(Application.Current.Dispatcher))
            {
                player.Play(mp3Path,1);await Task.Delay(150);player.PauseResume();var position=player.Position;await Task.Delay(200);Check("MP3 pause does not advance",player.Paused&&player.Position==position);player.PauseResume();await Task.Delay(150);player.Stop();Check("MP3 stop releases player",!player.Busy);
                var completion=new TaskCompletionSource();player.Completed+=()=>completion.TrySetResult();player.Play(wavePath,2);await completion.Task.WaitAsync(TimeSpan.FromSeconds(8));Check("WAV repeats and completion event",!player.Busy);
            }
            var db=Path.Combine(scratch,"carillon.db");
            using(var connection=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=db,Pooling=false}.ToString()))
            {connection.Open();using var command=connection.CreateCommand();using var stream=Application.GetResourceStream(new Uri("pack://application:,,,/samples/empty-schema.sql"))!.Stream;using var reader=new StreamReader(stream);command.CommandText=reader.ReadToEnd();command.ExecuteNonQuery();}
            var repository=new CarillonRepository(db);var due=DateTime.Now.AddSeconds(4);var song=new Song {Path=wavePath,From=due.Date,To=due.Date};song.Times[0].Value=due.ToString("HH:mm:ss");repository.SaveSong(song);
            runtime=new CarillonRuntime(repository,scratch,Application.Current.Dispatcher,true);
            clock=new ClockWindow {WindowState=WindowState.Normal,Width=900,Height=560};clock.Show();await Task.Delay(6500);
            Check("Scheduled file plays while analog Clock is open",runtime.Log.Count(DateTime.Today)==1&&clock.IsVisible&&!runtime.Player.Busy);
            Check("Playback completion keeps Played Song without redundant completion entry",runtime.Log.Read().Any(entry=>entry.Event.StartsWith("Played Song: ",StringComparison.Ordinal))&&!runtime.Log.Read().Any(entry=>entry.Event.Equals("Playback completed",StringComparison.OrdinalIgnoreCase)));
            runtime.SetSchedule(false);runtime.SetSchedule(true);await Task.Delay(1100);Check("Re-enable does not duplicate played occurrence",runtime.Log.Count(DateTime.Today)==1);
            runtime.Dispose();runtime=null;
            var fakeNow=DateTime.Now;var future=new Song {Path=wavePath,Playlist="Discontinuity",From=fakeNow.Date,To=fakeNow.Date};future.Times[0].Value=fakeNow.AddMinutes(10).ToString("HH:mm:ss");repository.SaveSong(future);
            runtime=new CarillonRuntime(repository,scratch,Application.Current.Dispatcher,true,()=>fakeNow);
            Check("Injected clock builds future queue",runtime.Queue.Any(b=>b.Song.Playlist=="Discontinuity"));
            fakeNow=fakeNow.AddHours(1);runtime.EvaluateCurrentTime();Check("Forward clock jump skips missed events",!runtime.Queue.Any(b=>b.Song.Playlist=="Discontinuity"));
            fakeNow=fakeNow.AddHours(-1);runtime.EvaluateCurrentTime();Check("Backward clock jump retains dispatched ledger",runtime.Queue.All(b=>b.Song.Playlist=="Discontinuity"));
            runtime.Settings["silence_enabled"]=1;runtime.Settings["ash_wed_begin"]=fakeNow.Date.ToString("yyyy-MM-dd");runtime.Settings["holy_thu_begin"]=fakeNow.Date.ToString("yyyy-MM-dd");runtime.Settings["holy_sat_end"]=fakeNow.Date.AddDays(2).ToString("yyyy-MM-dd");
            Check("Overlapping legacy silence reasons",runtime.SilenceReasons(fakeNow.Date.AddHours(12)).Count==2);
            Check("Legacy liturgical end boundary",!runtime.SilenceReasons(fakeNow.Date.AddHours(23).AddMinutes(55)).Contains("Ash Wednesday"));
            runtime.Settings["other_silence_begin_date"]=fakeNow.Date.ToString("yyyy-MM-dd");runtime.Settings["other_silence_end_date"]=fakeNow.Date.AddDays(1).ToString("yyyy-MM-dd");runtime.Settings["other_silence_begin_time"]="23:00";runtime.Settings["other_silence_end_time"]="01:00";
            Check("Funeral silence spans midnight",runtime.SilenceReasons(fakeNow.Date.AddDays(1).AddMinutes(30)).Contains("Funeral Silence"));runtime.Settings["silence_enabled"]=0;
            fakeNow=new DateTime(2027,1,2,0,16,0);runtime.EvaluateCurrentTime();Check("January 2 background liturgical maintenance",runtime.Repository.Settings()["ash_wed_begin"].ToString()=="2027-02-10");
            var groups=repository.Table("seasonalgroups");groups.Rows.Add("Easter","2026-04-05","2026-04-05","10:00");groups.Rows.Add("Custom","2026-02-03","2026-02-04","11:00");repository.SaveTable(groups,"SEASONALGROUP");
            fakeNow=new DateTime(2027,1,3,0,3,0);runtime.EvaluateCurrentTime();var recalculated=repository.Table("seasonalgroups");Check("January 3 background group maintenance",CarillonRepository.Text(recalculated.Rows[0],"PLAY_DATE_FROM")=="2027-03-28"&&CarillonRepository.Text(recalculated.Rows[1],"PLAY_DATE_FROM")=="2026-02-03");
            clock.Close();clock=new ClockWindow {WindowState=WindowState.Normal,Width=900,Height=560};clock.Show();await Task.Delay(200);Check("Clock reopens",clock.IsVisible);
            Check("Windows MPEG decoder not loaded",!Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(m=>m.ModuleName.Equals("msmpeg2ac3dec.dll",StringComparison.OrdinalIgnoreCase)));
            using(var endpointEnumerator=new NAudio.CoreAudioApi.MMDeviceEnumerator())
            using(var endpoint=endpointEnumerator.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render,NAudio.CoreAudioApi.Role.Multimedia))
            using(var systemAudio=new SystemAudio(Application.Current.Dispatcher))
            {
                var originalVolume=endpoint.AudioEndpointVolume.MasterVolumeLevelScalar;var originalMute=endpoint.AudioEndpointVolume.Mute;
                try {
                    systemAudio.RefreshDevice();var notifications=0;systemAudio.Changed+=()=>notifications++;
                    var changedVolume=originalVolume>.5f?originalVolume-.01f:originalVolume+.01f;endpoint.AudioEndpointVolume.MasterVolumeLevelScalar=changedVolume;await Task.Delay(300);
                    Check("External endpoint volume notifies application",notifications>0&&Math.Abs(systemAudio.Volume-changedVolume)<.002f);
                    systemAudio.Volume=originalVolume;await Task.Delay(150);Check("Application volume updates Windows endpoint",Math.Abs(endpoint.AudioEndpointVolume.MasterVolumeLevelScalar-originalVolume)<.002f);
                    endpoint.AudioEndpointVolume.Mute=true;await Task.Delay(150);systemAudio.SetSilenced(true);systemAudio.SetSilenced(false);Check("Preexisting manual mute restored",endpoint.AudioEndpointVolume.Mute);
                    endpoint.AudioEndpointVolume.Mute=false;await Task.Delay(150);systemAudio.SetSilenced(true);systemAudio.ToggleManualMute();systemAudio.SetSilenced(false);Check("Manual mute during silence retained",endpoint.AudioEndpointVolume.Mute);
                } finally {systemAudio.SetSilenced(false);endpoint.AudioEndpointVolume.MasterVolumeLevelScalar=originalVolume;endpoint.AudioEndpointVolume.Mute=originalMute;}
            }
            results.Add("PASS Audio backend: "+AudioPlayback.Backend);return 0;
        }
        catch(Exception error) {results.Add("FAIL "+error.Message);return 1;}
        finally {clock?.Close();runtime?.Dispose();File.WriteAllLines(Path.Combine(baseDirectory,"audio-regression-results.txt"),results);Directory.Delete(scratch,true);}
    }
}
