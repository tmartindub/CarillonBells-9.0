using MailKit;
using MailKit.Security;
using MimeKit;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Carillon;

public static class EmailReports
{
    [StructLayout(LayoutKind.Sequential)] private struct MemoryStatus { public uint Length,Load; public ulong TotalPhysical,AvailablePhysical,TotalPage,AvailablePage,TotalVirtual,AvailableVirtual,Extended; }
    [DllImport("kernel32.dll",SetLastError=true)] private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
    [DllImport("kernel32.dll",SetLastError=true)] private static extern bool GetSystemTimes(out long idle,out long kernel,out long user);
    private static long previousIdle,previousTotal;
    public static string Build(CarillonRuntime runtime)
    {
        var now=DateTime.Now; var schedule=runtime.FullDay(now); var logs=runtime.Log.Read(); var remaining=schedule.Where(b=>b.When>=now).ToList();
        var missing=runtime.Songs.Where(s=>!s.Title.Contains("Random Music Generator",StringComparison.OrdinalIgnoreCase)&&!File.Exists(DataFormats.PortablePath(runtime.BaseDirectory,s.Path))).ToList();
        var memory=new MemoryStatus {Length=(uint)Marshal.SizeOf<MemoryStatus>()}; GlobalMemoryStatusEx(ref memory);
        GetSystemTimes(out var idle,out var kernel,out var user); var total=kernel+user;
        var cpu=$"{100.0*(1.0-(double)(idle-previousIdle)/Math.Max(1,total-previousTotal)):F1}%"+(previousTotal==0?" average since boot":" since previous report sample"); previousIdle=idle; previousTotal=total;
        var html=new StringBuilder("<!doctype html><html><head><meta charset='utf-8'><title>Carillon health report</title></head><body style='font-family:Segoe UI;color:#173650'><h1>Carillon Morning Health Report</h1>");
        static string E(object? value)=>WebUtility.HtmlEncode(value?.ToString()??"None");
        void Row(string label,object? value)=>html.Append("<p><b>"+E(label)+":</b> "+E(value)+"</p>");
        void Section(string title,IEnumerable<string> lines) { html.Append("<h2>"+E(title)+"</h2><ul>"); foreach(var line in lines) html.Append("<li>"+E(line)+"</li>"); html.Append("</ul>"); }
        Row("Date / organization",$"{now:F} / {runtime.Setting("ORGANIZATION_NAME")}");
        foreach(var driveRoot in new[]{Path.GetPathRoot(runtime.BaseDirectory),Path.GetPathRoot(Environment.SystemDirectory)}.Distinct())
        { var drive=new DriveInfo(driveRoot!); if(drive.IsReady) Row("Drive",$"{drive.Name} {drive.VolumeLabel}; {drive.AvailableFreeSpace/1073741824.0:F1} GiB free / {drive.TotalSize/1073741824.0:F1} GiB"); }
        var boot=now-TimeSpan.FromMilliseconds(Environment.TickCount64); Row("Last Windows boot",boot); Row("Windows uptime",DataFormats.DurationText(now-boot));
        Row("CPU",cpu); Row("RAM",$"{memory.Load}% in use; {memory.AvailablePhysical/1048576} MiB available of {memory.TotalPhysical/1048576} MiB");
        Row("Application",$"Carillon Bells 9.0.0 / {RuntimeInformation.ProcessArchitecture}"); Row("Last startup",runtime.Started); Row("Application uptime",DataFormats.DurationText(now-runtime.Started));
        Row("Audio backend",AudioPlayback.Backend); Row("Output device",runtime.Audio.DeviceName);
        Row("Schedule",$"{schedule.Count} total today; {remaining.Count} remaining; {(runtime.ScheduleEnabled?"enabled":"paused")}");
        Row("First event",schedule.FirstOrDefault()); Row("Last event",schedule.LastOrDefault()); Row("Next event",remaining.FirstOrDefault());
        Row("Played today",runtime.Log.Count(now)); Row("Yesterday",$"{now.AddDays(-1):yyyy-MM-dd}: {runtime.Log.Count(now.AddDays(-1))} recorded plays");
        Row("Playlist",$"{runtime.Songs.Count} records; {DataFormats.DurationText(TimeSpan.FromTicks(runtime.Songs.Sum(s=>DataFormats.Duration(s.Duration).Ticks)))} total duration");
        Row("Logging",runtime.Log.Enabled?"Enabled":"Disabled; unrecorded plays cannot be reconstructed"); Row("Silence",runtime.SilenceLabel);
        var errors=logs.Where(l=>l.Timestamp?.Date==now.AddDays(-1).Date && l.Event.Contains("error",StringComparison.OrdinalIgnoreCase)).Select(l=>l.Original).ToList();
        Row("Overall health score",$"{Math.Max(0,100-Math.Min(50,missing.Count*5)-Math.Min(25,errors.Count*5)-(runtime.Audio.DeviceName.StartsWith("No default")?25:0))}/100");
        Section("Recent play activity",logs.Where(l=>l.Event.StartsWith("Played Song: ")).TakeLast(20).Reverse().Select(l=>l.Original));
        Section("Daily maintenance activity",logs.Where(l=>l.Event.Contains("maintenance",StringComparison.OrdinalIgnoreCase)||l.Event.Contains("recalculated",StringComparison.OrdinalIgnoreCase)||l.Event.Contains("trim",StringComparison.OrdinalIgnoreCase)).TakeLast(10).Reverse().Select(l=>l.Original));
        Section("Missing / broken song files",missing.Select(s=>s.Path)); Section("Previous-date errors",errors); Section("Remaining schedule",remaining.Select(b=>b.ToString()));
        html.Append("</body></html>"); return html.ToString();
    }
    public static async Task SendAsync(CarillonRuntime runtime,string subject,string html,string recipients,CancellationToken cancellation=default)
    {
        if(!runtime.Flag("email_enabled")) throw new InvalidOperationException("Email is disabled.");
        if(string.IsNullOrWhiteSpace(runtime.Setting("smtp_host"))) throw new ArgumentException("SMTP host is required.");
        if(!int.TryParse(runtime.Setting("smtp_port"),out var port)||port is <1 or >65535) throw new ArgumentException("A valid SMTP port is required.");
        var message=new MimeMessage();
        var from=runtime.Setting("from_address");var username=MailboxAddress.Parse(runtime.Setting("email_user"));
        message.From.Add(from.Contains('@')&&MailboxAddress.TryParse(from,out var configuredFrom)?configuredFrom:new MailboxAddress(from,username.Address));
        message.To.AddRange(InternetAddressList.Parse(recipients.Replace(';',','))); if(message.To.Count==0) throw new ArgumentException("A recipient is required.");
        message.Subject=subject; message.Body=new TextPart("html"){Text=html};
        using var client=new MailKit.Net.Smtp.SmtpClient {Timeout=15000};
        await client.ConnectAsync(runtime.Setting("smtp_host"),port,SecureSocketOptions.StartTls,cancellation);
        await client.AuthenticateAsync(runtime.Setting("email_user"),runtime.Setting("email_password"),cancellation);
        var result=await client.SendAsync(message,cancellation); await client.DisconnectAsync(true,cancellation);
        if(result.RejectedRecipients.Count>0) throw new InvalidOperationException("SMTP rejected one or more recipients; delivery was incomplete.");
        runtime.Log.Append("Email sent: "+subject);
    }
}
