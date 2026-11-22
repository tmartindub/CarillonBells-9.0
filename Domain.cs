using System.Globalization;

namespace Carillon;

public sealed class PlayDay
{
    public string Name { get; set; } = "";
    public bool Enabled { get; set; }
}
public sealed class PlayTime { public string Value { get; set; } = ""; }
public sealed class Song
{
    public string Playlist { get; set; } = "Daily List";
    public string Path { get; set; } = "";
    public string Title => System.IO.Path.GetFileNameWithoutExtension(Path);
    public string Group { get; set; } = "Daily List";
    public string Duration { get; set; } = "0:00";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Repeats { get; set; } = 1;
    public List<PlayDay> Days { get; set; } = Enumerable.Range(0,7).Select(i => new PlayDay { Name = DayNames[i], Enabled=true }).ToList();
    public List<PlayTime> Times { get; set; } = Enumerable.Range(0,12).Select(_ => new PlayTime()).ToList();
    public static readonly string[] DayNames = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"];
    public static readonly string[] DatabaseDays = ["monday","tuesday","wednesday","thursday","friday","saturday","sunday"];
    public string Key => Playlist + "\0" + Path;
    public Song Clone() => new() { Playlist=Playlist, Path=Path, Group=Group, Duration=Duration, From=From, To=To, Repeats=Repeats,
        Days=Days.Select(d=>new PlayDay { Name=d.Name, Enabled=d.Enabled }).ToList(), Times=Times.Select(t=>new PlayTime { Value=t.Value }).ToList() };
    public string Fingerprint => string.Join('|', Playlist, Path, Group, Duration, From, To, Repeats, string.Join(',',Days.Select(d=>d.Enabled)),string.Join(',',Times.Select(t=>t.Value)));
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path) || string.IsNullOrWhiteSpace(Playlist)) throw new ArgumentException("A playlist name and song path are required.");
        if (From > To) throw new ArgumentException("From must be on or before Until.");
        if (Repeats is <1 or >5) throw new ArgumentException("Times to play must be from 1 through 5.");
        foreach (var time in Times) if (!string.IsNullOrWhiteSpace(time.Value)) time.Value=DataFormats.Time(time.Value)!.Value.ToString("HH:mm:ss",CultureInfo.InvariantCulture);
    }
}

public static class DataFormats
{
    public static DateTime? Date(object? value)
    {
        if (value is null or DBNull || string.IsNullOrWhiteSpace(value.ToString())) return null;
        if (value is double or float or long or int) return DateTime.FromOADate(Convert.ToDouble(value)).Date;
        var text=value.ToString()!.Trim();
        if (DateTime.TryParse(text,CultureInfo.InvariantCulture,DateTimeStyles.AllowWhiteSpaces,out var date) || DateTime.TryParse(text,CultureInfo.GetCultureInfo("en-US"),DateTimeStyles.AllowWhiteSpaces,out date)) return date.Date;
        throw new FormatException("Invalid stored date: " + text);
    }
    public static TimeOnly? Time(object? value)
    {
        if (value is null or DBNull || string.IsNullOrWhiteSpace(value.ToString())) return null;
        if (value is double or float or long or int) return TimeOnly.FromDateTime(DateTime.FromOADate(Convert.ToDouble(value)));
        var text=value.ToString()!.Trim().Replace('.',':');
        if (TimeOnly.TryParse(text,CultureInfo.InvariantCulture,DateTimeStyles.AllowWhiteSpaces,out var time) || TimeOnly.TryParse(text,CultureInfo.GetCultureInfo("en-US"),DateTimeStyles.AllowWhiteSpaces,out time)) return time;
        if (DateTime.TryParse(value.ToString(),CultureInfo.InvariantCulture,DateTimeStyles.AllowWhiteSpaces,out var date)) return TimeOnly.FromDateTime(date);
        throw new FormatException("Invalid time. Use HH:mm[:ss] or h:mm AM/PM.");
    }
    public static TimeSpan Duration(string text)
    {
        var parts=text.Trim().Split(':');
        if (parts.Length is <2 or >3 || parts.Any(p=>!int.TryParse(p,out var n)||n<0)) return TimeSpan.Zero;
        var values=parts.Select(int.Parse).ToArray();
        if (values[^1]>59 || (parts.Length==3 && values[1]>59)) return TimeSpan.Zero;
        return parts.Length==2 ? TimeSpan.FromSeconds(values[0]*60L+values[1]) : TimeSpan.FromSeconds(values[0]*3600L+values[1]*60L+values[2]);
    }
    public static string DurationText(TimeSpan value) => value.TotalHours>=1 ? $"{(long)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}" : $"{(long)value.TotalMinutes}:{value.Seconds:00}";
    public static string PortablePath(string baseDirectory,string path) => System.IO.Path.GetFullPath(System.IO.Path.IsPathRooted(path)?path:System.IO.Path.Combine(baseDirectory,path));
}

public sealed record ScheduledBell(DateTime When, Song Song)
{
    public string Id => When.ToString("yyyyMMddHHmmss",CultureInfo.InvariantCulture)+"\0"+Song.Key;
    public override string ToString() => $"{When:HH:mm:ss}   {Song.Title}   ×{Math.Clamp(Song.Repeats,1,5)}";
}
public sealed record SilenceRule(int Id, bool Enabled, string Purpose, TimeOnly? Start, TimeOnly? End, bool[] Days)
{
    public (DateTime Start,DateTime End)? Occurrence(DateTime startDate)
    {
        if (!Enabled || Start is null || End is null || Start==End || !Days[((int)startDate.DayOfWeek+6)%7]) return null;
        var begin=startDate.Date+Start.Value.ToTimeSpan();
        return (begin,startDate.Date+End.Value.ToTimeSpan()+(End<Start?TimeSpan.FromDays(1):TimeSpan.Zero));
    }
    public bool Contains(DateTime now,bool endInclusive=false) => new[]{now.Date.AddDays(-1),now.Date}.Any(date => Occurrence(date) is {} window && now>=window.Start && (endInclusive?now<=window.End:now<window.End));
}
public static class ScheduleBuilder
{
    public static List<ScheduledBell> Build(IEnumerable<Song> songs,DateTime now,IEnumerable<SilenceRule> silence,bool fullDay=false)
    {
        var rules=silence.ToArray(); var weekday=((int)now.DayOfWeek+6)%7;
        return songs.Where(s=>s.From is not null && s.To is not null && now.Date>=s.From.Value.Date && now.Date<=s.To.Value.Date && s.Days[weekday].Enabled)
            .SelectMany(song=>song.Times.Select(t=>DataFormats.Time(t.Value)).Where(t=>t.HasValue).Distinct().Select(t=>new ScheduledBell(now.Date+t!.Value.ToTimeSpan(),song)))
            .Where(b=>(fullDay || b.When>=now) && !rules.Any(r=>r.Contains(b.When,true)))
            .OrderBy(b=>b.When).ThenBy(b=>b.Song.Playlist,StringComparer.Ordinal).ThenBy(b=>b.Song.Path,StringComparer.Ordinal).ToList();
    }
}
public static class CalendarRules
{
    public static DateTime Easter(int year)
    {
        int a=year%19,b=year/100,c=year%100,d=b/4,e=b%4,f=(b+8)/25,g=(b-f+1)/3,h=(19*a+b-d-g+15)%30,i=c/4,k=c%4,l=(32+2*e+2*i-h-k)%7,m=(a+11*h+22*l)/451;
        return new DateTime(year,(h+l-7*m+114)/31,(h+l-7*m+114)%31+1);
    }
    public static (DateTime From,DateTime To)? GroupDates(string name,DateTime today)
    {
        (DateTime,DateTime)? InYear(int year)
        {
            DateTime Fixed(int month,int day)=>new(year,month,day);
            DateTime date;
            switch(name.Trim().ToLowerInvariant())
            {
                case "christmas": return (Fixed(12,22),Fixed(12,26));
                case "christmas eve": date=Fixed(12,24); break;
                case "easter": date=Easter(year); break;
                case "epiphany": date=Fixed(1,6); break;
                case "independence day": date=Fixed(7,4); break;
                case "memorial day": date=Fixed(5,31); date=date.AddDays(-(((int)date.DayOfWeek+6)%7)); break;
                case "new year's day": date=Fixed(1,1); break;
                case "nine eleven": date=Fixed(9,11); break;
                case "thanksgiving": date=Fixed(11,1); date=date.AddDays(((4-(int)date.DayOfWeek+7)%7)+21); break;
                case "valentine's day": date=Fixed(2,14); break;
                case "veterans day": date=Fixed(11,11); break;
                default: return null;
            }
            return (date,date);
        }
        var range=InYear(today.Year); return range is {} r && r.Item2<today.Date?InYear(today.Year+1):range;
    }
    public static bool AnnualMatch(DateTime today,DateTime from,DateTime to)
    {
        int current=today.Month*100+today.Day,start=from.Month*100+from.Day,end=to.Month*100+to.Day;
        return start<=end?current>=start&&current<=end:current>=start||current<=end;
    }
}
