using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Carillon;

public sealed class CarillonRuntime : IDisposable
{
    public CarillonRepository Repository { get; }
    public AudioPlayback Player { get; }
    public SystemAudio Audio { get; }
    public CarillonLog Log { get; }
    public string BaseDirectory { get; }
    public bool Evaluation { get; }
    public DateTime Started { get; }
    public int ClockHandStyleId { get; private set; }
    private const int LastClockHandStyleId = 18;
    private string ClockHandsFile => Path.Combine(BaseDirectory, "data", "clock-hands.txt");
    public string UptimeLabel
    {
        get
        {
            var now = Now;
            if (now < Started) now = Started;

            // Count complete months from the original start date, including month-end anniversaries.
            var completeMonths = (now.Year - Started.Year) * 12 + now.Month - Started.Month;
            if (Started.AddMonths(completeMonths) > now) completeMonths--;
            var remainder = now - Started.AddMonths(completeMonths);
            var units = new (int Count, string Name)[]
            {
                (completeMonths / 12, "year"), (completeMonths % 12, "month"),
                (remainder.Days / 7, "week"), (remainder.Days % 7, "day"),
                (remainder.Hours, "hour"), (remainder.Minutes, "minute"), (remainder.Seconds, "second")
            };
            return string.Join("   ", units.Select(unit => $"{unit.Count} {unit.Name}{(unit.Count == 1 ? "" : "s")}"));
        }
    }
    public List<Song> Songs { get; private set; } = [];
    public List<SilenceRule> Rules { get; private set; } = [];
    public Dictionary<string,object> Settings { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ScheduledBell> Queue { get; private set; } = [];
    public bool ScheduleEnabled { get; private set; }
    public string SilenceLabel { get; private set; } = "No active silence";
    public string LastPlayed { get; private set; } = "No songs played this session";
    public int PlayedToday { get; private set; }
    public string Error { get; private set; } = "";
    public event Action? Changed;
    private readonly DispatcherTimer timer;
    private readonly Stopwatch monotonic=Stopwatch.StartNew();
    private DateTime lastWall=DateTime.Now;
    private TimeSpan lastElapsed;
    private DateTime rebuildDate,maintenanceDate,trimDate,reportDate;
    private int settingsYear,groupsYear;
    private string previousSilence="";
    private bool ticking,disposed;
    private readonly HashSet<string> dispatched=[];
    private readonly CancellationTokenSource lifetime=new();
    private readonly Func<DateTime> currentTime;
    public DateTime Now=>currentTime();
    private readonly List<Task> background=[];
    private readonly bool migrationReview;
    [DllImport("kernel32.dll")] private static extern uint SetThreadExecutionState(uint flags);
    public CarillonRuntime(CarillonRepository repository,string baseDirectory,Dispatcher dispatcher,bool evaluation,Func<DateTime>? currentTime=null,bool migrationReview=false)
    {
        Repository=repository; BaseDirectory=baseDirectory; Evaluation=evaluation||migrationReview; this.currentTime=currentTime??(()=>DateTime.Now);
        this.migrationReview=migrationReview;
        Started=Now;
        try
        {
            if (File.Exists(ClockHandsFile) && int.TryParse(File.ReadAllText(ClockHandsFile), out var id) && id is >= 0 and <= LastClockHandStyleId)
                ClockHandStyleId = id;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            // A missing or inaccessible display preference leaves the clock on Original.
        }
        Log=new(baseDirectory); Player=new(dispatcher); Audio=new(dispatcher);
        Reload(); repository.EnsureRuntimeTables(backupInDatabaseFolder:true);
        foreach(var id in repository.Dispatched(Now.Date)) dispatched.Add(id);
        Log.Trim(Now); trimDate=Now.Date; PlayedToday=Log.Count(Now.Date);
        reportDate=Now.Hour>=5?Now.Date:DateTime.MinValue;
        Player.Completed+=()=>Changed?.Invoke();
        Player.Failed+=Fail; Audio.Changed+=()=>Changed?.Invoke(); Audio.RefreshDevice();
        SetThreadExecutionState(0x80000001);
        Log.Append("Application initialized — "+App.ProductTitle+" · "+AudioPlayback.Backend);
        timer=new DispatcherTimer(TimeSpan.FromSeconds(1),DispatcherPriority.Background,Tick,dispatcher);
        SetSchedule(!migrationReview); Tick(null,EventArgs.Empty);
    }
    public void SaveClockHandStyle(int styleId)
    {
        if (styleId is < 0 or > LastClockHandStyleId) throw new ArgumentOutOfRangeException(nameof(styleId));
        if (styleId == ClockHandStyleId) return;
        var path = ClockHandsFile;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        try
        {
            File.WriteAllText(temporary, styleId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            File.Move(temporary, path, overwrite: true);
            ClockHandStyleId = styleId;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
    public string Setting(string name)=>Settings.TryGetValue(name,out var v)&&v is not DBNull?Convert.ToString(v,System.Globalization.CultureInfo.InvariantCulture)??"":"";
    public bool Flag(string name)=>Setting(name)=="1";
    public void Reload()
    {
        Songs=Repository.Songs(); Rules=Repository.Silence(); Settings=Repository.Settings(); Log.Enabled=Flag("LOGGING_STATUS"); Changed?.Invoke();
    }
    public void SetSchedule(bool enabled)
    {
        ScheduleEnabled=enabled;
        if(enabled) { Repository.SyncGroups(); Reload(); Rebuild(Now); }
        else Queue.Clear();
        Log.Append("Schedule "+(enabled?"enabled":"disabled")); Changed?.Invoke();
    }
    public void Rebuild(DateTime now)
    {
        Queue=ScheduleBuilder.Build(Songs,now,Rules).Where(b=>!dispatched.Contains(b.Id)).ToList();
        rebuildDate=now.Date;
    }
    public List<ScheduledBell> FullDay(DateTime now)=>ScheduleBuilder.Build(Songs,now,Rules,true);
    public void Play(Song song)
    {
        var path=DataFormats.PortablePath(BaseDirectory,song.Path);
        if(song.Title.Contains("Random Music Generator",StringComparison.OrdinalIgnoreCase))
        {
            var folder=Repository.ResolveRandomDirectory(DateTime.Now,BaseDirectory);
            var files=Directory.GetFiles(folder,"*.mp3");
            if(files.Length==0) throw new IOException("The active random music directory contains no MP3 files.");
            path=files[Random.Shared.Next(files.Length)];
        }
        if(!File.Exists(path)) throw new FileNotFoundException("Music file not found:\n"+path+"\n\nDatabase import does not copy audio files. Copy the music folder into this installation so the file is available at the path shown above.",path);
        Player.Play(path,song.Repeats);
        Log.Append("Played Song: "+path); if(Log.Enabled) PlayedToday++; LastPlayed="Last song played: "+Path.GetFileNameWithoutExtension(path); Changed?.Invoke();
    }
    public List<string> SilenceReasons(DateTime now)
    {
        var reasons=Rules.Where(r=>r.Contains(now)).Select(r=>r.Purpose).ToList();
        if(!Flag("silence_enabled")) return reasons;
        DateTime? D(string key)=>DataFormats.Date(Settings.GetValueOrDefault(key));
        bool Period(DateTime? start,DateTime? end)=>start is {} a&&end is {} b&&now>=a&&now<b;
        if(Period(D("ash_wed_begin")?.AddHours(2).AddMinutes(5),D("ash_wed_begin")?.AddHours(23).AddMinutes(55))) reasons.Add("Ash Wednesday");
        if(Period(D("holy_thu_begin")?.AddHours(2).AddMinutes(5),D("holy_sat_end")?.AddHours(23).AddMinutes(55))) reasons.Add("Holy Thursday - Saturday");
        if(Period(D("all_souls_day")?.AddHours(2).AddMinutes(5),D("all_souls_day")?.AddHours(23).AddMinutes(55))) reasons.Add("All Souls Day");
        var begin=D("other_silence_begin_date"); var finish=D("other_silence_end_date");
        var beginTime=DataFormats.Time(Settings.GetValueOrDefault("other_silence_begin_time")); var endTime=DataFormats.Time(Settings.GetValueOrDefault("other_silence_end_time"));
        if(beginTime is {} bt&&endTime is {} et&&Period(begin+bt.ToTimeSpan(),finish+et.ToTimeSpan())) reasons.Add("Funeral Silence");
        return reasons;
    }
    private void Tick(object? sender,EventArgs e)
    {
        if(ticking||disposed) return; ticking=true;
        try
        {
            var now=Now; var elapsed=monotonic.Elapsed;
            var gap=elapsed-lastElapsed; var jump=(now-lastWall)-gap;
            Audio.RefreshDevice();
            if(gap.TotalSeconds>10 || Math.Abs(jump.TotalSeconds)>2 || now.Date!=lastWall.Date)
            {
                if(now.Date!=lastWall.Date) PlayedToday=Log.Count(now.Date);
                if(ScheduleEnabled) Rebuild(now);
                Log.Append("Schedule rebuilt after date/time discontinuity; missed occurrences skipped");
            }
            lastElapsed=elapsed; lastWall=now;
            var reasons=SilenceReasons(now); var silence=string.Join("; ",reasons.Distinct());
            var applyAutomation=!migrationReview||ScheduleEnabled;
            Audio.SetSilenced(applyAutomation&&reasons.Count>0);
            SilenceLabel=silence.Length==0?"No active silence":silence;
            var appliedSilence=applyAutomation?silence:"";
            if(previousSilence!=appliedSilence)
            {
                var message=appliedSilence.Length==0?"Silence ended; previous manual mute restored":"Silence started: "+appliedSilence;
                Log.Append(message); previousSilence=appliedSilence;
                if(Flag("email_enabled")&&!Evaluation) Track(EmailReports.SendAsync(this,App.ProductTitle+" - System Alert",System.Net.WebUtility.HtmlEncode(message),Setting("email_recipients"),lifetime.Token,recipientLabel:"Alert recipients"));
            }
            if(now.TimeOfDay>=TimeSpan.FromMinutes(5)&&maintenanceDate!=now.Date&&ScheduleEnabled)
            {
                maintenanceDate=now.Date; Log.Append("Total Songs played yesterday: "+Log.Count(now.Date.AddDays(-1))); Repository.SyncGroups(); Reload(); Rebuild(now); Log.Append("Daily schedule maintenance completed");
            }
            if(now.TimeOfDay>=TimeSpan.FromMinutes(10)&&trimDate!=now.Date) { Log.Trim(now); trimDate=now.Date; Log.Append("Daily 30-day log trim completed"); }
            if(applyAutomation&&now.Month==1&&now.Day>=2&&now.TimeOfDay>=TimeSpan.FromMinutes(15)&&settingsYear!=now.Year) { RecalculateSettings(); settingsYear=now.Year; Log.Append("Annual liturgical dates recalculated"); }
            if(applyAutomation&&now.Month==1&&now.Day==3&&now.TimeOfDay>=TimeSpan.FromMinutes(2)&&groupsYear!=now.Year) { RecalculateGroups(); groupsYear=now.Year; Log.Append("Annual seasonal dates recalculated"); }
            if(now.Hour>=5&&reportDate!=now.Date)
            {
                reportDate=now.Date;
                if(Flag("email_enabled")&&!Evaluation) Track(EmailReports.SendAsync(this,App.ProductTitle+" - Morning Health Report",EmailReports.Build(this),Setting("morning_report_recipients"),lifetime.Token,recipientLabel:"Morning report recipients"));
                else if(Evaluation) Log.Append("Morning report suppressed in evaluation mode");
            }
            if(now.Second%30==0) { var data=Path.Combine(BaseDirectory,"data"); Directory.CreateDirectory(data); File.WriteAllText(Path.Combine(data,"keepalive.txt"),now.ToString("O")); }
            if(ScheduleEnabled&&!Player.Busy&&Queue.Count>0&&Queue[0].When<=now)
            {
                var bell=Queue[0]; Queue.RemoveAt(0);
                if(dispatched.Add(bell.Id))
                {
                    Repository.RecordDispatch(bell.Id,bell.When);
                    if(reasons.Count==0) Play(bell.Song); else Log.Append("Skipped scheduled song during silence: "+bell.Song.Title);
                }
            }
            Changed?.Invoke();
        }
        catch(Exception error) { Fail(error.Message); }
        finally { ticking=false; }
    }
    public void EvaluateCurrentTime()=>Tick(null,EventArgs.Empty);
    private void Track(Task task)
    {
        background.RemoveAll(t=>t.IsCompleted); background.Add(task);
        _=Observe(task);
    }
    private async Task Observe(Task task) { try { await task; } catch(OperationCanceledException) { } catch(Exception e) { Fail("Email failed: "+e.Message); } }
    public void Fail(string message) { Error=message; try { Log.Append("ERROR: "+message); } catch(IOException) { } Changed?.Invoke(); }
    public void EmailSucceeded()
    {
        if (!Error.StartsWith("Email failed:",StringComparison.Ordinal)) return;
        Error=""; Changed?.Invoke();
    }
    public void RecalculateGroups()
    {
        var table=Repository.Table("seasonalgroups");
        foreach(DataRow row in table.Rows) if(CalendarRules.GroupDates(CarillonRepository.Text(row,"SEASONALGROUP"),Now.Date) is {} dates) { row["PLAY_DATE_FROM"]=dates.From.ToString("yyyy-MM-dd"); row["PLAY_DATE_TO"]=dates.To.ToString("yyyy-MM-dd"); }
        Repository.SaveTable(table,"SEASONALGROUP"); Repository.SyncGroups(); Reload(); if(ScheduleEnabled) Rebuild(Now);
    }
    public void RecalculateSettings()
    {
        var settings=Repository.Settings(); var easter=CalendarRules.Easter(Now.Date.Year); if(easter<Now.Date) easter=CalendarRules.Easter(Now.Date.Year+1);
        settings["ash_wed_begin"]=easter.AddDays(-46).ToString("yyyy-MM-dd"); settings["holy_thu_begin"]=easter.AddDays(-3).ToString("yyyy-MM-dd"); settings["holy_sat_end"]=easter.AddDays(-1).ToString("yyyy-MM-dd");
        var souls=new DateTime(Now.Date.Year,11,2); if(souls<Now.Date) souls=souls.AddYears(1); settings["all_souls_day"]=souls.ToString("yyyy-MM-dd");
        Repository.SaveSettings(settings,["ash_wed_begin","holy_thu_begin","holy_sat_end","all_souls_day"]); Reload();
    }
    public void Dispose()
    {
        if(disposed) return; disposed=true; timer.Stop(); lifetime.Cancel(); Player.Dispose(); Audio.Dispose(); SetThreadExecutionState(0x80000000); Log.Append("Application closed"); lifetime.Dispose();
    }
}
