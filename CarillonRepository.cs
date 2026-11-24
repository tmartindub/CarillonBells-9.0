using Microsoft.Data.Sqlite;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace Carillon;

public sealed class CarillonRepository(string databasePath)
{
    public string DatabasePath { get; } = System.IO.Path.GetFullPath(databasePath);
    public static readonly string[] SystemFields = ["ORGANIZATION_NAME","RAND_SONGS_DIRECTORY","LOGGING_STATUS","silence_enabled","ash_wed_begin","ash_wed_end","holy_thu_begin","holy_sat_end","all_souls_day","other_silence_begin_date","other_silence_end_date","other_silence_begin_time","other_silence_end_time"];
    public static readonly string[] EmailFields = ["email_enabled","email_recipients","email_user","email_password","smtp_host","smtp_port","from_address","test_recipient","morning_report_recipients"];
    public SqliteConnection Open(bool readOnly=false)
    {
        var connection=new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=DatabasePath,Mode=readOnly?SqliteOpenMode.ReadOnly:SqliteOpenMode.ReadWrite,Pooling=false,DefaultTimeout=10 }.ToString());
        connection.Open(); return connection;
    }
    public DataTable Table(string table)
    {
        if (!new[]{"playlist","seasonalgroups","PL_SETTINGS","PL_RAND_DIR_ROTATION","silence_schedule"}.Contains(table)) throw new ArgumentException("Unknown table.");
        using var connection=Open(true); using var command=connection.CreateCommand();
        command.CommandText=$"SELECT * FROM [{table}]";
        using var reader=command.ExecuteReader(); var result=new DataTable(table); result.Load(reader); return result;
    }
    public List<Song> Songs() => Table("playlist").Rows.Cast<DataRow>().Select(row => new Song {
        Playlist=Text(row,"playlist_name"),Path=Text(row,"song_name"),Group=Text(row,"season"),Duration=Text(row,"song_duration"),
        From=DataFormats.Date(row["play_date_from"]),To=DataFormats.Date(row["play_date_to"]),Repeats=Math.Clamp(Int(row,"num_times_to_play"),1,5),
        Days=Song.DatabaseDays.Select((d,i)=>new PlayDay {Name=Song.DayNames[i],Enabled=Int(row,"play_"+d)!=0}).ToList(),
        Times=Enumerable.Range(1,12).Select(i=>new PlayTime {Value=DataFormats.Time(row["scheduled_time"+i])?.ToString("HH:mm:ss")??""}).ToList()
    }).OrderBy(s=>s.Group).ThenBy(s=>s.Title).ToList();
    public static string Text(DataRow row,string field) => row.IsNull(field)?"":Convert.ToString(row[field],CultureInfo.InvariantCulture)??"";
    public static int Int(DataRow row,string field) => int.TryParse(Text(row,field),out var value)?value:0;
    public List<SilenceRule> Silence() => Table("silence_schedule").Rows.Cast<DataRow>().Select(row=>new SilenceRule(Int(row,"silence_id"),Int(row,"enabled")!=0,Text(row,"Purpose"),DataFormats.Time(row["start_time"]),DataFormats.Time(row["end_time"]),Song.DatabaseDays.Select(d=>Int(row,d)!=0).ToArray())).OrderBy(r=>r.Id).ToList();
    public Dictionary<string,object> Settings() => Table("PL_SETTINGS").Rows.Cast<DataRow>().Where(r=>Int(r,"settings_id")==1).Select(r=>r.Table.Columns.Cast<DataColumn>().ToDictionary(c=>c.ColumnName,c=>r[c],StringComparer.OrdinalIgnoreCase)).Single();
    public void SaveSong(Song song,Song? original=null)
    {
        song.Validate(); using var connection=Open(); using var transaction=connection.BeginTransaction();
        using var command=connection.CreateCommand(); command.Transaction=transaction;
        var values=new Dictionary<string,object?> { ["playlist_name"]=song.Playlist,["song_name"]=song.Path,["season"]=song.Group,["play_date_from"]=song.From?.ToString("yyyy-MM-dd"),["play_date_to"]=song.To?.ToString("yyyy-MM-dd"),["num_times_to_play"]=song.Repeats,["song_duration"]=song.Duration };
        for(var i=0;i<7;i++) values["play_"+Song.DatabaseDays[i]]=song.Days[i].Enabled?1:0;
        for(var i=0;i<12;i++) values["scheduled_time"+(i+1)]=DataFormats.Time(song.Times[i].Value)?.ToString("HH:mm:ss");
        var fields=values.Keys.ToArray();
        command.CommandText=original is null?$"INSERT INTO playlist ({string.Join(',',fields)}) VALUES ({string.Join(',',fields.Select((_,i)=>"@p"+i))})":$"UPDATE playlist SET {string.Join(',',fields.Select((f,i)=>f+"=@p"+i))} WHERE playlist_name=@oldPlaylist AND song_name=@oldPath";
        for(var i=0;i<fields.Length;i++) command.Parameters.AddWithValue("@p"+i,values[fields[i]]??DBNull.Value);
        if(original is not null) { command.Parameters.AddWithValue("@oldPlaylist",original.Playlist); command.Parameters.AddWithValue("@oldPath",original.Path); }
        if(command.ExecuteNonQuery()!=1) throw new DBConcurrencyException("The song changed or was removed. Refresh before editing.");
        transaction.Commit();
    }
    public void DeleteSong(Song song)
    {
        using var connection=Open(); using var command=connection.CreateCommand(); command.CommandText="DELETE FROM playlist WHERE playlist_name=@playlist AND song_name=@song";
        command.Parameters.AddWithValue("@playlist",song.Playlist); command.Parameters.AddWithValue("@song",song.Path); command.ExecuteNonQuery();
    }
    public void SaveSettings(IReadOnlyDictionary<string,object> values,IEnumerable<string> ownedFields)
    {
        var fields=ownedFields.Distinct().ToArray();
        if(fields.Contains("ash_wed_begin")&&!fields.Contains("ash_wed_end")) fields=[..fields,"ash_wed_end"];
        if(fields.Any(f=>!SystemFields.Concat(EmailFields).Concat(new[]{"form_bgcolor","form_fontcolor"}).Contains(f))) throw new ArgumentException("Invalid settings field.");
        using var connection=Open(); using var transaction=connection.BeginTransaction(); using var command=connection.CreateCommand(); command.Transaction=transaction;
        command.CommandText=$"UPDATE PL_SETTINGS SET {string.Join(',',fields.Select((f,i)=>f+"="+(f=="ash_wed_end"&&!fields.Contains("ash_wed_begin")?"ash_wed_begin":"@p"+i)))} WHERE settings_id=1";
        for(var i=0;i<fields.Length;i++)
        {
            if(fields[i]=="ash_wed_end"&&!fields.Contains("ash_wed_begin")) continue;
            command.Parameters.AddWithValue("@p"+i,Normalize(fields[i],values[fields[i]=="ash_wed_end"?"ash_wed_begin":fields[i]])??DBNull.Value);
        }
        if(command.ExecuteNonQuery()!=1) throw new DBConcurrencyException("Exactly one settings row with settings_id=1 is required.");
        transaction.Commit();
    }
    public void SaveTable(DataTable table,string key)
    {
        if(!new[]{("seasonalgroups","SEASONALGROUP"),("PL_RAND_DIR_ROTATION","slot_no"),("silence_schedule","silence_id")}.Contains((table.TableName,key))) throw new ArgumentException("Invalid table/key.");
        using var connection=Open(); using var transaction=connection.BeginTransaction();
        foreach(DataRow row in table.Rows)
        {
            if(row.RowState==DataRowState.Unchanged) continue;
            using var command=connection.CreateCommand(); command.Transaction=transaction;
            if(row.RowState==DataRowState.Deleted)
            {
                command.CommandText=$"DELETE FROM [{table.TableName}] WHERE [{key}]=@key";
                command.Parameters.AddWithValue("@key",row[key,DataRowVersion.Original]); command.ExecuteNonQuery(); continue;
            }
            var fields=table.Columns.Cast<DataColumn>().Select(c=>c.ColumnName).ToArray();
            if(row.RowState==DataRowState.Added) command.CommandText=$"INSERT INTO [{table.TableName}] ({string.Join(',',fields.Select(f=>$"[{f}]"))}) VALUES ({string.Join(',',fields.Select((_,i)=>"@p"+i))})";
            else { command.CommandText=$"UPDATE [{table.TableName}] SET {string.Join(',',fields.Select((f,i)=>$"[{f}]=@p{i}"))} WHERE [{key}]=@key"; command.Parameters.AddWithValue("@key",row[key,DataRowVersion.Original]); }
            for(var i=0;i<fields.Length;i++) command.Parameters.AddWithValue("@p"+i,Normalize(fields[i],row[fields[i]])??DBNull.Value);
            command.ExecuteNonQuery();
        }
        transaction.Commit(); table.AcceptChanges();
    }
    private static object? Normalize(string field,object value)
    {
        if(value is DBNull) return null;
        var f=field.ToLowerInvariant();
        if(f is "frm_date" or "to_date")
        {
            var text=Convert.ToString(value,CultureInfo.InvariantCulture)?.Trim()??"";
            return (DateTime.TryParseExact(text,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var stored)?stored:DataFormats.AnnualDate(value))?.ToString("yyyy-MM-dd");
        }
        if(f.Contains("date") || f is "frm_date" or "to_date" or "all_souls_day" or "ash_wed_begin" or "ash_wed_end" or "holy_thu_begin" or "holy_sat_end") return DataFormats.Date(value)?.ToString("yyyy-MM-dd");
        if(f.Contains("time") && f!="num_times_to_play") return DataFormats.Time(value)?.ToString("HH:mm:ss");
        return value;
    }
    public void SyncGroups()
    {
        using var connection=Open(); using var transaction=connection.BeginTransaction();
        foreach(DataRow row in Table("seasonalgroups").Rows)
        {
            var name=Text(row,"SEASONALGROUP").Trim(); if(name.Equals("Daily List",StringComparison.OrdinalIgnoreCase)) continue;
            using var command=connection.CreateCommand(); command.Transaction=transaction;
            command.CommandText="UPDATE playlist SET play_date_from=@from,play_date_to=@to,scheduled_time1=@time,"+string.Join(',',Enumerable.Range(2,11).Select(i=>$"scheduled_time{i}=NULL"))+" WHERE TRIM(season)=@season COLLATE NOCASE";
            command.Parameters.AddWithValue("@from",(object?)DataFormats.Date(row["PLAY_DATE_FROM"])?.ToString("yyyy-MM-dd")??DBNull.Value);
            command.Parameters.AddWithValue("@to",(object?)DataFormats.Date(row["PLAY_DATE_TO"])?.ToString("yyyy-MM-dd")??DBNull.Value);
            command.Parameters.AddWithValue("@time",(object?)DataFormats.Time(row["PLAY_TIME"])?.ToString("HH:mm:ss")??DBNull.Value);
            command.Parameters.AddWithValue("@season",name); command.ExecuteNonQuery();
        }
        transaction.Commit();
    }
    public string ResolveRandomDirectory(DateTime now,string baseDirectory)
    {
        var rows=Table("PL_RAND_DIR_ROTATION").Rows.Cast<DataRow>().OrderBy(r=>Int(r,"slot_no")).ToList();
        var chosen=rows.FirstOrDefault(r=>Int(r,"slot_no")>1 && !string.IsNullOrWhiteSpace(Text(r,"rand_music_dir")) && DataFormats.Date(r["frm_date"]) is {} from && DataFormats.Date(r["to_date"]) is {} to && CalendarRules.AnnualMatch(now,from,to)) ?? rows.FirstOrDefault(r=>Int(r,"slot_no")==1);
        var path=chosen is null?"":Text(chosen,"rand_music_dir"); if(string.IsNullOrWhiteSpace(path)) throw new IOException("No random music directory is configured.");
        return DataFormats.PortablePath(baseDirectory,path);
    }
    public string BackupZip(bool inDatabaseFolder=false)
    {
        var folder=Path.GetDirectoryName(DatabasePath)!;
        if(!inDatabaseFolder) { folder=Path.Combine(folder,"backup"); Directory.CreateDirectory(folder); }
        var zipPath=Path.Combine(folder,"carillon_before_upgrade_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".zip");
        var snapshot=Path.Combine(folder,Guid.NewGuid()+".db");
        try {
            using(var source=Open()) using(var target=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=snapshot,Pooling=false}.ToString())) { target.Open(); source.BackupDatabase(target); }
            using var zip=ZipFile.Open(zipPath,ZipArchiveMode.Create); zip.CreateEntryFromFile(snapshot,"carillon.db",CompressionLevel.Optimal);
        } finally { if(File.Exists(snapshot)) File.Delete(snapshot); }
        return zipPath;
    }
    public void EnsureRuntimeTables(bool backupInDatabaseFolder=false,bool alreadyBackedUp=false)
    {
        using var connection=Open(); using var command=connection.CreateCommand();
        command.CommandText="SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Carillon9_Dispatched'";
        if(!alreadyBackedUp&&Convert.ToInt32(command.ExecuteScalar())==0) BackupZip(backupInDatabaseFolder);
        command.CommandText="CREATE TABLE IF NOT EXISTS Carillon9_Dispatched(event_id TEXT PRIMARY KEY,event_date TEXT NOT NULL)"; command.ExecuteNonQuery();
    }
    public IEnumerable<string> Dispatched(DateTime day)
    {
        using var connection=Open(true); using var command=connection.CreateCommand(); command.CommandText="SELECT event_id FROM Carillon9_Dispatched WHERE event_date=@day"; command.Parameters.AddWithValue("@day",day.ToString("yyyy-MM-dd"));
        using var reader=command.ExecuteReader(); while(reader.Read()) yield return reader.GetString(0);
    }
    public void RecordDispatch(string id,DateTime when)
    {
        using var connection=Open(); using var command=connection.CreateCommand(); command.CommandText="INSERT OR IGNORE INTO Carillon9_Dispatched VALUES(@id,@day)"; command.Parameters.AddWithValue("@id",id); command.Parameters.AddWithValue("@day",when.ToString("yyyy-MM-dd")); command.ExecuteNonQuery();
    }
    public int NormalizeLegacyTimes()
    {
        using var connection=Open();using var transaction=connection.BeginTransaction();var changed=0;
        var domains=new[]{("playlist",Enumerable.Range(1,12).Select(i=>"scheduled_time"+i).ToArray()),("seasonalgroups",new[]{"PLAY_TIME"}),("PL_SETTINGS",new[]{"other_silence_begin_time","other_silence_end_time"}),("silence_schedule",new[]{"start_time","end_time"})};
        foreach(var (table,columns) in domains)foreach(var column in columns)
        {
            var values=new List<(long Row,string Value)>();
            using(var read=connection.CreateCommand()){read.Transaction=transaction;read.CommandText=$"SELECT rowid,[{column}] FROM [{table}] WHERE [{column}] IS NOT NULL";using var reader=read.ExecuteReader();while(reader.Read()){var value=DataFormats.Time(reader.GetValue(1));if(value is not null&&reader.GetValue(1).ToString()!=value.Value.ToString("HH:mm:ss"))values.Add((reader.GetInt64(0),value.Value.ToString("HH:mm:ss")));}}
            foreach(var item in values){using var update=connection.CreateCommand();update.Transaction=transaction;update.CommandText=$"UPDATE [{table}] SET [{column}]=@value WHERE rowid=@row";update.Parameters.AddWithValue("@value",item.Value);update.Parameters.AddWithValue("@row",item.Row);changed+=update.ExecuteNonQuery();}
        }
        transaction.Commit();return changed;
    }
    public void EnsureCompatibleSchema(bool backupInDatabaseFolder=false,bool alreadyBackedUp=false)
    {
        using var connection=Open();
        HashSet<string> Fields(string table) { using var cmd=connection.CreateCommand(); cmd.CommandText=$"PRAGMA table_info([{table}])"; using var reader=cmd.ExecuteReader(); var fields=new HashSet<string>(StringComparer.OrdinalIgnoreCase); while(reader.Read()) fields.Add(reader.GetString(1)); return fields; }
        var settings=Fields("PL_SETTINGS"); var rotation=Fields("PL_RAND_DIR_ROTATION"); var silence=Fields("silence_schedule");
        var missingSettings=SystemFields.Concat(EmailFields).Concat(new[]{"settings_id","form_bgcolor","form_fontcolor"}).Where(f=>!settings.Contains(f)).ToArray();
        var wide=rotation.Count>0&&!rotation.Contains("slot_no");
        using var settingsCheck=connection.CreateCommand();settingsCheck.CommandText="PRAGMA index_list(PL_SETTINGS)";
        var stableIndex=false;using(var indexReader=settingsCheck.ExecuteReader()){while(indexReader.Read())if(indexReader.GetString(1)=="idx_pl_settings_settings_id"&&indexReader.GetInt32(2)==1)stableIndex=true;}
        var stableRows=false;if(settings.Contains("settings_id")){settingsCheck.CommandText="SELECT COUNT(*) FROM PL_SETTINGS";var totalRows=Convert.ToInt32(settingsCheck.ExecuteScalar());settingsCheck.CommandText="SELECT COUNT(*) FROM PL_SETTINGS WHERE settings_id=1";stableRows=totalRows==1&&Convert.ToInt32(settingsCheck.ExecuteScalar())==1;}
        var ashDatesMatch=true;
        if(settings.Contains("ash_wed_begin")&&settings.Contains("ash_wed_end")) {settingsCheck.CommandText="SELECT COUNT(*) FROM PL_SETTINGS WHERE ash_wed_end IS NOT ash_wed_begin";ashDatesMatch=Convert.ToInt32(settingsCheck.ExecuteScalar())==0;}
        if(missingSettings.Length==0&&!wide&&rotation.Contains("slot_no")&&silence.Contains("Purpose")&&stableIndex&&stableRows&&ashDatesMatch) return;
        if(!alreadyBackedUp) BackupZip(backupInDatabaseFolder);
        using var transaction=connection.BeginTransaction();
        void Run(string sql) { using var cmd=connection.CreateCommand(); cmd.Transaction=transaction; cmd.CommandText=sql; cmd.ExecuteNonQuery(); }
        if(settings.Count==0){Run("CREATE TABLE PL_SETTINGS(settings_id INTEGER NOT NULL DEFAULT 1)");settings.Add("settings_id");}
        foreach(var f in missingSettings.Where(f=>!settings.Contains(f))) Run($"ALTER TABLE PL_SETTINGS ADD COLUMN [{f}] {(f=="settings_id"?"INTEGER NOT NULL DEFAULT 1":f is "email_enabled" or "LOGGING_STATUS" or "silence_enabled" or "smtp_port"?"INTEGER":"TEXT")}");
        using(var count=connection.CreateCommand()) { count.Transaction=transaction; count.CommandText="SELECT COUNT(*) FROM PL_SETTINGS"; if(Convert.ToInt32(count.ExecuteScalar())>1) throw new InvalidDataException("Multiple settings rows require review; no rows were discarded."); }
        Run("UPDATE PL_SETTINGS SET settings_id=1; CREATE UNIQUE INDEX IF NOT EXISTS idx_pl_settings_settings_id ON PL_SETTINGS(settings_id)");
        Run($"INSERT INTO PL_SETTINGS(settings_id,ORGANIZATION_NAME,LOGGING_STATUS,email_enabled,silence_enabled) SELECT 1,'{App.ProductName}',1,0,0 WHERE NOT EXISTS(SELECT 1 FROM PL_SETTINGS)");
        Run("UPDATE PL_SETTINGS SET ash_wed_end=ash_wed_begin WHERE ash_wed_end IS NOT ash_wed_begin");
        if(wide)
        {
            var backupName="PL_RAND_DIR_ROTATION_WIDE_BACKUP";
            if(Fields(backupName).Count>0) backupName+="_"+DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Run("ALTER TABLE PL_RAND_DIR_ROTATION RENAME TO "+backupName);
            Run("CREATE TABLE PL_RAND_DIR_ROTATION(slot_no INTEGER PRIMARY KEY CHECK(slot_no BETWEEN 1 AND 7),rand_music_dir TEXT,frm_date DATE,to_date DATE)");
            for(int i=1;i<=7;i++) Run($"INSERT INTO PL_RAND_DIR_ROTATION SELECT {i},rand_music_dir{i},frm_date{i},to_date{i} FROM {backupName} WHERE id=1");
        }
        else if(rotation.Count==0) Run("CREATE TABLE PL_RAND_DIR_ROTATION(slot_no INTEGER PRIMARY KEY CHECK(slot_no BETWEEN 1 AND 7),rand_music_dir TEXT,frm_date DATE,to_date DATE)");
        for(int i=1;i<=7;i++) Run($"INSERT OR IGNORE INTO PL_RAND_DIR_ROTATION(slot_no) VALUES({i})");
        if(silence.Count==0) Run("CREATE TABLE silence_schedule(silence_id INTEGER PRIMARY KEY,enabled INTEGER NOT NULL DEFAULT 0,Purpose TEXT,start_time TEXT,end_time TEXT,"+string.Join(',',Song.DatabaseDays.Select(d=>d+" INTEGER NOT NULL DEFAULT 0"))+")");
        else if(!silence.Contains("Purpose")) Run("ALTER TABLE silence_schedule ADD COLUMN Purpose TEXT");
        for(int i=1;i<=12;i++) Run($"INSERT OR IGNORE INTO silence_schedule(silence_id,Purpose) VALUES({i},'Silence Window {i}')");
        transaction.Commit();
    }
}
