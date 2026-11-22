using System.Globalization;
using System.IO;

namespace Carillon;

public sealed record LogEntry(DateTime? Timestamp,string Event,string Original);
public sealed class CarillonLog(string baseDirectory)
{
    private readonly object sync=new();
    public bool Enabled { get; set; }
    public string FilePath { get; } = Path.Combine(baseDirectory,"logs","CarillonPlayLog.txt");
    public static LogEntry Parse(string line)
    {
        var split=line.IndexOf(" - ",StringComparison.Ordinal);
        if(split<0) return new(null,line,line);
        var text=line[..split];
        DateTime? stamp=DateTime.TryParseExact(text,"yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date) || DateTime.TryParse(text,CultureInfo.GetCultureInfo("en-US"),DateTimeStyles.None,out date)?date:null;
        return new(stamp,line[(split+3)..],line);
    }
    public List<LogEntry> Read()
    {
        lock(sync) return File.Exists(FilePath)?File.ReadAllLines(FilePath).Select(Parse).ToList():[];
    }
    public int Count(DateTime day)=>Read().Count(e=>e.Timestamp?.Date==day.Date&&e.Event.StartsWith("Played Song: ",StringComparison.Ordinal));
    public void Append(string message)
    {
        if(!Enabled) return;
        lock(sync) { Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!); File.AppendAllText(FilePath,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)+" - "+message.Replace('\r',' ').Replace('\n',' ')+Environment.NewLine); }
    }
    public void Trim(DateTime now)
    {
        lock(sync)
        {
            if(!File.Exists(FilePath)) return;
            var kept=File.ReadAllLines(FilePath).Where(l=>Parse(l).Timestamp is not {} t || t>=now.AddDays(-30)).ToArray();
            var temp=FilePath+".trim";
            try { File.WriteAllLines(temp,kept); File.Move(temp,FilePath,true); } finally { if(File.Exists(temp)) File.Delete(temp); }
        }
    }
}
