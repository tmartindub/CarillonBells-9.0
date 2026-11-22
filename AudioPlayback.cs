using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Windows.Threading;

namespace Carillon;

public sealed class AudioPlayback(Dispatcher dispatcher) : IDisposable
{
    private WaveStream? reader;
    private WaveOutEvent? output;
    private int repeats;
    public event Action? Completed;
    public event Action<string>? Failed;
    public bool Busy => output is not null;
    public bool Paused => output?.PlaybackState==PlaybackState.Paused;
    public string CurrentTitle { get; private set; } = "Ready";
    public TimeSpan Position => reader?.CurrentTime??TimeSpan.Zero;
    public TimeSpan Duration => reader?.TotalTime??TimeSpan.Zero;
    public const string Backend = "NLayer managed MP3 / PCM WAV → NAudio waveOut";
    public static WaveStream OpenFile(string path)
    {
        switch(Path.GetExtension(path).ToLowerInvariant())
        {
            case ".mp3": return new Mp3FileReaderBase(path,waveFormat=>new NLayer.NAudioSupport.Mp3FrameDecompressor(waveFormat));
            case ".wav":
                var wave=new WaveFileReader(path);
                if(wave.WaveFormat.Encoding is not WaveFormatEncoding.Pcm and not WaveFormatEncoding.IeeeFloat and not WaveFormatEncoding.Extensible) { wave.Dispose(); throw new NotSupportedException("WAV must contain PCM or IEEE float audio."); }
                return wave;
            default: throw new NotSupportedException("Select an MP3 or PCM WAV file.");
        }
    }
    public static TimeSpan ReadDuration(string path) { using var stream=OpenFile(path); return stream.TotalTime; }
    public void Play(string path,int repeatCount)
    {
        Stop();
        try {
            reader=OpenFile(path); output=new WaveOutEvent { DesiredLatency=150 };
            output.Init(reader.ToSampleProvider().ToWaveProvider16());
            repeats=Math.Clamp(repeatCount,1,5); CurrentTitle=Path.GetFileNameWithoutExtension(path);
            output.PlaybackStopped+=OnStopped; output.Play();
        } catch { Stop(); throw; }
    }
    private void OnStopped(object? sender,StoppedEventArgs e) => dispatcher.BeginInvoke(()=>{
        if(sender!=output || output is null) return;
        if(e.Exception is not null) { var message=e.Exception.Message; Stop(); Failed?.Invoke(message); return; }
        if(--repeats>0 && reader is not null) { reader.Position=0; output.Play(); return; }
        Stop(); Completed?.Invoke();
    });
    public void PauseResume() { if(output?.PlaybackState==PlaybackState.Playing) output.Pause(); else if(output?.PlaybackState==PlaybackState.Paused) output.Play(); }
    public void Stop()
    {
        var previous=output; output=null;
        if(previous is not null) { previous.PlaybackStopped-=OnStopped; previous.Stop(); previous.Dispose(); }
        reader?.Dispose(); reader=null; repeats=0; CurrentTitle="Ready";
    }
    public void Dispose()=>Stop();
}

public sealed class SystemAudio(Dispatcher dispatcher) : IDisposable
{
    private readonly MMDeviceEnumerator enumerator=new();
    private MMDevice? device;
    private bool managed;
    private bool manualMute;
    public string DeviceName => device?.FriendlyName??"No default output device";
    public float Volume { get => device?.AudioEndpointVolume.MasterVolumeLevelScalar??0; set { if(device is not null) device.AudioEndpointVolume.MasterVolumeLevelScalar=Math.Clamp(value,0,1); } }
    public bool Muted => device?.AudioEndpointVolume.Mute??false;
    public event Action? Changed;
    public void RefreshDevice()
    {
        MMDevice? next=null;
        try { next=enumerator.GetDefaultAudioEndpoint(DataFlow.Render,Role.Multimedia); }
        catch(System.Runtime.InteropServices.COMException) { }
        if(next?.ID==device?.ID) { next?.Dispose(); return; }
        if(device is not null) { device.AudioEndpointVolume.OnVolumeNotification-=OnVolume; if(managed) device.AudioEndpointVolume.Mute=manualMute; device.Dispose(); }
        device=next;
        if(device is not null) { manualMute=device.AudioEndpointVolume.Mute; device.AudioEndpointVolume.OnVolumeNotification+=OnVolume; if(managed) device.AudioEndpointVolume.Mute=true; }
        Changed?.Invoke();
    }
    private void OnVolume(AudioVolumeNotificationData data) => dispatcher.BeginInvoke(()=>{
        if(!managed) manualMute=data.Muted;
        else if(!data.Muted && device is not null) { manualMute=false; device.AudioEndpointVolume.Mute=true; }
        Changed?.Invoke();
    });
    public void ToggleManualMute() { manualMute=!manualMute; if(device is not null) device.AudioEndpointVolume.Mute=managed||manualMute; Changed?.Invoke(); }
    public void SetSilenced(bool active)
    {
        if(managed==active) return;
        if(active) manualMute=Muted;
        managed=active;
        if(device is not null) device.AudioEndpointVolume.Mute=managed||manualMute;
        Changed?.Invoke();
    }
    public void Dispose()
    {
        if(device is not null) { device.AudioEndpointVolume.OnVolumeNotification-=OnVolume; if(managed) device.AudioEndpointVolume.Mute=manualMute; device.Dispose(); device=null; }
        enumerator.Dispose();
    }
}
