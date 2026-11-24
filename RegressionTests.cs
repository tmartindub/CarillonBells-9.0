using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Carillon.Migration;
namespace Carillon;
public static class RegressionTests
{
    public static int Run(string baseDirectory)
    {
        var results=new List<string>();
        void Check(string name,bool condition) { if(!condition) throw new Exception(name); results.Add("PASS "+name); }
        try {
            var now=new DateTime(2026,11,23,12,0,0); var song=new Song {Path="test.mp3",From=now.Date,To=now.Date,Repeats=1};
            song.Times[0].Value="00:00";song.Times[1].Value="12:00";song.Times[2].Value="23:59:59";song.Times[3].Value="12:00";
            Check("Full day, midnight, duplicate slots",ScheduleBuilder.Build([song],now,[],true).Count==3);
            Check("Only remaining events",ScheduleBuilder.Build([song],now,[]).Count==2);
            var other=song.Clone();other.Path="other.mp3";Check("Simultaneous distinct records",ScheduleBuilder.Build([song,other],now,[],true).Count==6);
            song.Days[0].Enabled=false;Check("Disabled weekday",ScheduleBuilder.Build([song],now,[],true).Count==0);song.Days[0].Enabled=true;
            song.To=now.AddDays(-1);Check("Expired range",ScheduleBuilder.Build([song],now,[],true).Count==0);song.To=now;
            var rule=new SilenceRule(1,true,"Service",new TimeOnly(11,0),new TimeOnly(12,0),[true,false,false,false,false,false,false]);
            Check("Compile silence end inclusive",ScheduleBuilder.Build([song],now,[rule],true).Count==2);
            Check("Live silence end exclusive",!rule.Contains(now));
            var overnight=rule with {Start=new TimeOnly(23,0),End=new TimeOnly(1,0)};
            Check("Overnight starting weekday",overnight.Contains(now.Date.AddDays(1).AddMinutes(30))&&!overnight.Contains(now.Date.AddDays(2).AddMinutes(30)));
            Check("Equal silence start/end inactive",!(rule with{Start=rule.End}).Contains(now));
            Check("Cross-year directory override",CalendarRules.AnnualMatch(new(2027,1,4),new(2026,12,1),new(2027,1,5)));
            Check("Christmas range retained while active",CalendarRules.GroupDates("Christmas",new(2026,12,24))?.From==new DateTime(2026,12,22));
            Check("Custom groups preserved",CalendarRules.GroupDates("Custom",now) is null);
            Check("Easter 2027",CalendarRules.Easter(2027)==new DateTime(2027,3,28));
            Check("Memorial Day",CalendarRules.GroupDates("Memorial Day",new(2026,1,1))?.From==new DateTime(2026,5,25));
            Check("Thanksgiving",CalendarRules.GroupDates("Thanksgiving",new(2026,1,1))?.From==new DateTime(2026,11,26));
            Check("Duration m:ss",DataFormats.Duration("1:30").TotalSeconds==90);
            Check("Duration >24 hours",DataFormats.Duration("28:30:00").TotalHours==28.5);
            Check("NULL versus midnight",DataFormats.Time(DBNull.Value) is null&&DataFormats.Time("00:00")==TimeOnly.MinValue);
            Check("AM/PM input",DataFormats.Time("10:05 PM")==new TimeOnly(22,5));
            Check("Legacy log timestamp",CarillonLog.Parse("11/23/2026 12:00:00 PM - Played Song: test").Timestamp==now);
            Check("Invalid historical date retained",CarillonLog.Parse("2/29/2026 12:00:00 PM - test").Timestamp is null);
            Check("Hour and minute geometry",((11%12)*30+59*0.5)==359.5);
            var handStyles = (ClockHandStyle[])Application.Current.FindResource("ClockHandCatalog");
            Check("Original and the fourteen numbered clock hand styles have renderable geometry",
                handStyles.Select(style => style.Id).SequenceEqual(Enumerable.Range(0, 15)) &&
                handStyles.All(style => style.Hour.Drawing is not null && style.Minute.Drawing is not null && style.Cap.Drawing is not null));
            var originalHands = handStyles[0];
            Check("Original clock hand proportions and pivot match the previous clock",
                originalHands.Minute.Drawing.Bounds == new Rect(220, 13, 20, 217) &&
                originalHands.Hour.Drawing.Bounds == new Rect(220, 60, 20, 170) &&
                originalHands.Cap.Drawing.Bounds == new Rect(215, 215, 30, 30));
            using var icon = Application.GetResourceStream(new Uri("pack://application:,,,/images and icons/Carillon-Bells-9.0-icon.ico"))?.Stream;
            Check("The approved window icon is an embedded WPF resource", icon?.Length > 0);
            Check("Portable relative audio follows EXE folder",DataFormats.PortablePath(@"D:\Carillon9",@".\Songs\bell.mp3")==@"D:\Carillon9\Songs\bell.mp3");
            Check("Legacy drive letter follows relocated EXE",DataFormats.PortablePath(@"E:\Copied9",@"C:\Songs\bell.mp3")==@"E:\Copied9\Songs\bell.mp3");
            Check("Rooted audio follows EXE folder",DataFormats.PortablePath(@"E:\Copied9",@"\Music\bell.mp3")==@"E:\Copied9\Music\bell.mp3");
            foreach(var escaped in new[]{@"..\outside.mp3",@"C:outside.mp3",@"\\server\share\bell.mp3"}) { var rejected=false; try { DataFormats.PortablePath(baseDirectory,escaped); } catch(ArgumentException) { rejected=true; } Check("Reject nonportable audio "+escaped,rejected); }
            var palettes=(CarillonPalette[])Application.Current.FindResource("ThemeCatalog");
            var cardStyle=(Style)Application.Current.FindResource("Card");
            foreach(var palette in palettes)
            {
                ThemeManager.Apply(palette);
                var card=new Border {Style=cardStyle};
                var tint=((SolidColorBrush)card.Background).Color;
                Check($"{palette.Name} cards use a tint separate from the canvas and controls",
                    tint==(Color)ColorConverter.ConvertFromString(palette.CardSurface!) &&
                    tint!=((SolidColorBrush)Application.Current.Resources["Canvas"]).Color &&
                    tint!=((SolidColorBrush)Application.Current.Resources["Surface"]).Color);
            }
            foreach(var background in new[]{"#F0F0F0","#252525"})
            {
                ThemeManager.Apply(new CarillonPalette {Canvas=background,Surface=background});
                var card=new Border {Style=cardStyle};
                Check($"Custom canvas {background} gets a distinct card tint",
                    ((SolidColorBrush)card.Background).Color!=((SolidColorBrush)Application.Current.Resources["Canvas"]).Color);
            }
            ThemeManager.Apply(palettes[0]);
            TestDatabase(baseDirectory,Check);
            File.WriteAllLines(Path.Combine(baseDirectory,"regression-results.txt"),results); return 0;
        } catch(Exception error) { results.Add("FAIL "+error.Message); File.WriteAllLines(Path.Combine(baseDirectory,"regression-results.txt"),results); return 1; }
    }
    private static void TestDatabase(string baseDirectory,Action<string,bool> check)
    {
        var scratch=Path.Combine(baseDirectory,"test-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(scratch);
        try
        {
            var path=Path.Combine(scratch,"carillon.db");
            using(var connection=new Microsoft.Data.Sqlite.SqliteConnection(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder {DataSource=path,Pooling=false}.ToString()))
            {
                connection.Open(); using var command=connection.CreateCommand();
                using var stream=System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/samples/empty-schema.sql"))!.Stream; using var reader=new StreamReader(stream); command.CommandText=reader.ReadToEnd(); command.ExecuteNonQuery();
                command.CommandText="CREATE TABLE CustomTable(value TEXT); INSERT INTO CustomTable VALUES('preserve')"; command.ExecuteNonQuery();
            }
            var repository=new CarillonRepository(path); repository.EnsureCompatibleSchema();
            check("New databases default to portable random songs",
                repository.Settings()["RAND_SONGS_DIRECTORY"].ToString() == @".\random_songs"
                && repository.ResolveRandomDirectory(new(2027,6,1),scratch) == Path.Combine(scratch,"random_songs"));
            var song=new Song {Path="test.mp3",From=DateTime.Today,To=DateTime.Today}; song.Times[0].Value="00:00"; repository.SaveSong(song);
            var loaded=repository.Songs().Single(); check("SQLite NULL versus midnight",loaded.Times[0].Value=="00:00:00"&&loaded.Times[1].Value=="");
            using(var connection=repository.Open()){using var command=connection.CreateCommand();command.CommandText="UPDATE playlist SET scheduled_time2='1899-12-30 14:05:00'";command.ExecuteNonQuery();}check("Legacy date-time normalization",repository.NormalizeLegacyTimes()==1&&repository.Songs().Single().Times[1].Value=="14:05:00");
            var edited=loaded.Clone();edited.Repeats=3;repository.SaveSong(edited,loaded); check("SQLite composite-key update",repository.Songs().Single().Repeats==3);
            bool duplicate=false;try {repository.SaveSong(song);}catch(Microsoft.Data.Sqlite.SqliteException){duplicate=true;}check("SQLite duplicate detection",duplicate);
            var system=repository.Settings();var email=repository.Settings(); system["ORGANIZATION_NAME"]="System saved";repository.SaveSettings(system,["ORGANIZATION_NAME"]);email["smtp_host"]="example.invalid";repository.SaveSettings(email,["smtp_host"]);system["LOGGING_STATUS"]=0;repository.SaveSettings(system,["LOGGING_STATUS"]);
            check("System Email System field ownership",repository.Settings()["ORGANIZATION_NAME"].ToString()=="System saved"&&repository.Settings()["smtp_host"].ToString()=="example.invalid");
            var groups=repository.Table("seasonalgroups");groups.Rows.Add("Easter","2027-03-28","2027-03-28","10:00");repository.SaveTable(groups,"SEASONALGROUP");
            check("Group CRUD",repository.Table("seasonalgroups").Rows.Count==1);groups.Rows[0].Delete();repository.SaveTable(groups,"SEASONALGROUP");check("Group deletion",repository.Table("seasonalgroups").Rows.Count==0);
            repository.EnsureRuntimeTables();repository.RecordDispatch("id",DateTime.Today);repository.RecordDispatch("id",DateTime.Today);check("Persistent dispatch deduplication",repository.Dispatched(DateTime.Today).Count()==1);
            var rotation=repository.Table("PL_RAND_DIR_ROTATION");rotation.Rows[0]["rand_music_dir"]="default";rotation.Rows[1]["rand_music_dir"]="override";rotation.Rows[1]["frm_date"]="2026-12-01";rotation.Rows[1]["to_date"]="2027-01-10";repository.SaveTable(rotation,"slot_no");
            check("Rotation cross-year override",repository.ResolveRandomDirectory(new(2027,1,1),scratch)==Path.Combine(scratch,"override"));check("Rotation fallback",repository.ResolveRandomDirectory(new(2027,6,1),scratch)==Path.Combine(scratch,"default"));
            var backup=repository.BackupZip();using(var zip=ZipFile.OpenRead(backup))check("SQLite consistent ZIP backup",zip.GetEntry("carillon.db")!.Length>0);
            check("Row-based database opens without the 7.2.4 upgrade",!MigrationEngine.LocalDatabaseRequiresUpgrade(path));
            using(var connection=repository.Open()) {using var command=connection.CreateCommand();command.CommandText="DROP TABLE PL_RAND_DIR_ROTATION; CREATE TABLE PL_RAND_DIR_ROTATION(id INTEGER PRIMARY KEY,"+string.Join(',',Enumerable.Range(1,7).Select(i=>$"rand_music_dir{i} TEXT,frm_date{i} DATE,to_date{i} DATE"))+"); INSERT INTO PL_RAND_DIR_ROTATION(id,rand_music_dir1) VALUES(1,'wide-default'); DROP TABLE silence_schedule;";command.ExecuteNonQuery();}
            check("Wide 7.2.4 database is detected",MigrationEngine.LocalDatabaseRequiresUpgrade(path));
            var beforeUpgrade = File.ReadAllBytes(path);
            var directoriesBeforeUpgrade = Directory.GetDirectories(scratch);
            var upgradeBackup = MigrationEngine.UpgradeLocalDatabase(path);
            using (var zip = ZipFile.OpenRead(upgradeBackup))
            using (var entry = zip.GetEntry("carillon.db")!.Open())
            using (var savedDatabase = new MemoryStream())
            {
                entry.CopyTo(savedDatabase);
                check("In-place upgrade ZIP retains the exact old database",Path.GetDirectoryName(upgradeBackup)==scratch&&savedDatabase.ToArray().SequenceEqual(beforeUpgrade));
            }
            check("In-place upgrade leaves no new folders or staging files",Directory.GetDirectories(scratch).SequenceEqual(directoriesBeforeUpgrade)&&!Directory.EnumerateFiles(scratch,".carillon-upgrade-*").Any());
            check("In-place upgrade retains play times, weekday flags, and dispatch records",repository.Songs().Single().Fingerprint==edited.Fingerprint&&repository.Dispatched(DateTime.Today).Single()=="id");
            check("Wide migration and 12 silence slots",repository.Table("PL_RAND_DIR_ROTATION").Rows.Count==7&&repository.Table("silence_schedule").Rows.Count==12&&!MigrationEngine.LocalDatabaseRequiresUpgrade(path));
            using(var connection=repository.Open(true)){using var command=connection.CreateCommand();command.CommandText="SELECT value FROM CustomTable";check("Unknown tables survive migration",command.ExecuteScalar()?.ToString()=="preserve");command.CommandText="SELECT COUNT(*) FROM sqlite_master WHERE name LIKE 'PL_RAND_DIR_ROTATION_WIDE_BACKUP%'";check("Existing wide backup preserved",Convert.ToInt32(command.ExecuteScalar())==2);command.CommandText="PRAGMA quick_check";check("SQLite integrity after migration",command.ExecuteScalar()?.ToString()=="ok");}
            var restoreInstall = Path.Combine(scratch, "restore-usb");
            var restoreFolder = Directory.CreateDirectory(Path.Combine(restoreInstall, "databases")).FullName;
            var restoreDatabase = Path.Combine(restoreFolder, "carillon.db");
            File.WriteAllBytes(restoreDatabase, beforeUpgrade);
            var originalZip = MigrationEngine.UpgradeLocalDatabase(restoreDatabase);
            var upgradedDatabase = File.ReadAllBytes(restoreDatabase);
            var choices = MigrationEngine.DiscoverRestoreBackups(restoreInstall);
            check("Restore finds the original startup backup on the selected USB",
                choices.Count == 1 && choices[0].ZipPath == originalZip);

            var corruptZip = Path.Combine(scratch, "corrupt.zip");
            File.WriteAllText(corruptZip, "Invalid ZIP");
            var rejectedCorruptBackup = false;
            try { MigrationEngine.PlanRestore724(restoreInstall, corruptZip); }
            catch (InvalidDataException) { rejectedCorruptBackup = true; }
            check("Restore rejects damaged ZIPs without changing the database",
                rejectedCorruptBackup && File.ReadAllBytes(restoreDatabase).SequenceEqual(upgradedDatabase));

            var restorePlan = MigrationEngine.PlanRestore724(restoreInstall, originalZip);
            var restoreResult = MigrationEngine.Restore724(restorePlan);
            using (var zip = ZipFile.OpenRead(restoreResult.PreRestoreBackupPath))
            using (var entry = zip.GetEntry("carillon.db")!.Open())
            using (var saved = new MemoryStream())
            {
                entry.CopyTo(saved);
                check("Restore saves the exact v9 database before replacement", saved.ToArray().SequenceEqual(upgradedDatabase));
            }
            check("Restore reinstates the exact 7.2.4 database", restoreResult.DatabasePath == restoreDatabase &&
                File.ReadAllBytes(restoreDatabase).SequenceEqual(beforeUpgrade) &&
                MigrationEngine.LocalDatabaseRequiresUpgrade(restoreDatabase));
            check("Restore keeps the original backup in place", File.Exists(originalZip));

            repository.DeleteSong(repository.Songs().Single());check("Song deletion",repository.Songs().Count==0);
            var log=new CarillonLog(scratch){Enabled=true};log.Append("Played Song: one");log.Append("Daily play count checkpoint: 45");check("Played count ignores checkpoints",log.Count(DateTime.Today)==1);log.Enabled=false;log.Append("Played Song: hidden");check("Logging disabled respected",log.Count(DateTime.Today)==1);
            File.AppendAllLines(log.FilePath,["2/29/2026 12:00:00 PM - malformed",DateTime.Now.AddDays(-31).ToString("yyyy-MM-dd HH:mm:ss")+" - old"]);log.Trim(DateTime.Now);check("Log trim preserves malformed lines",log.Read().Any(l=>l.Timestamp is null)&&!log.Read().Any(l=>l.Event=="old"));
            repository.SaveSong(new Song {Group="Custom",Path="first.mp3",From=DateTime.Today,To=DateTime.Today});
            repository.SaveSong(new Song {Group="Custom",Path="second.mp3",From=DateTime.Today,To=DateTime.Today});
            var uptimeStart = new DateTime(2024, 2, 29, 1, 2, 3);
            var clockNow = uptimeStart;
            using var runtime=new CarillonRuntime(repository,scratch,System.Windows.Threading.Dispatcher.CurrentDispatcher,true,()=>clockNow);
            check("Uptime starts at zero in all seven units",
                runtime.UptimeLabel == "0 years   0 months   0 weeks   0 days   0 hours   0 minutes   0 seconds");
            clockNow = DateTime.Now;
            repository.SaveSettings(new Dictionary<string,object>{{"ash_wed_begin","01/02/2027"}},["ash_wed_begin"]);
            var dateGroups=repository.Table("seasonalgroups"); dateGroups.Rows.Add("Date test","01/02/2027","12/31/2027","10:00"); repository.SaveTable(dateGroups,"SEASONALGROUP");
            check("U.S. month-first input persists as ISO",repository.Settings()["ash_wed_begin"].ToString()=="2027-01-02"&&repository.Table("seasonalgroups").Rows[0]["PLAY_DATE_FROM"].ToString()=="2027-01-02");
            using(var connection=repository.Open()) { using var command=connection.CreateCommand(); command.CommandText="UPDATE seasonalgroups SET PLAY_TIME='10:00:01.000'"; command.ExecuteNonQuery(); }
            var dateWindow=new AdministrationWindow(runtime,"ClockHands");
            check("Group time display omits fractions without changing storage",dateWindow.Groups.Rows[0]["PLAY_TIME"].ToString()=="10:00:01"&&repository.Table("seasonalgroups").Rows[0]["PLAY_TIME"].ToString()=="10:00:01.000");
            check("Administration shows slash dates without pending changes",dateWindow.Groups.Rows[0]["PLAY_DATE_FROM"].ToString()=="01/02/2027"&&dateWindow.Groups.GetChanges() is null);
            var dateSettings=(System.Data.DataTable)typeof(AdministrationWindow).GetField("settings",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(dateWindow)!;
            check("System dates display U.S. format with blanks retained",dateSettings.Rows[0]["ash_wed_begin"].ToString()=="01/02/2027"&&dateSettings.Rows[0].IsNull("other_silence_begin_date")&&dateSettings.GetChanges() is null);
            var adminFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var cancelHands = typeof(AdministrationWindow).GetMethod("CancelClick", adminFlags)!;
            try
            {
                var picker = (ListBox)dateWindow.FindName("ClockHandPicker");
                var preference = Path.Combine(scratch, "data", "clock-hands.txt");
                check("Clock hands opens on Original without pausing scheduling",
                    Equals(picker.SelectedValue, 0) && !dateWindow.RequiresScheduleResume && runtime.ScheduleEnabled);
                picker.SelectedValue = 14;
                check("Previewing a clock style leaves the saved preference alone",
                    Equals(picker.SelectedValue, 14) && runtime.ClockHandStyleId == 0 && !File.Exists(preference));

                var saveHands = typeof(AdministrationWindow).GetMethod("Save", adminFlags)!;
                check("Saving a clock style stores its ID and updates the analog clock resource",
                    (bool)saveHands.Invoke(dateWindow, null)! && runtime.ClockHandStyleId == 14 &&
                    File.ReadAllText(preference) == "14" && ((ClockHandStyle)Application.Current.Resources["SelectedClockHands"]).Id == 14);
                picker.SelectedValue = 10;
                cancelHands.Invoke(dateWindow, [null, new RoutedEventArgs()]);
                check("Cancel returns the preview to the saved style and keeps scheduling enabled",
                    Equals(picker.SelectedValue, 14) && File.ReadAllText(preference) == "14" && runtime.ScheduleEnabled);
            }
            finally
            {
                cancelHands.Invoke(dateWindow, [null, new RoutedEventArgs()]);
                dateWindow.Close();
                Application.Current.Resources.Remove("SelectedClockHands");
            }
            check("Legacy ISO and U.S. log dates parse",CarillonLog.Parse("2026-01-02 12:00:00 - legacy").Timestamp==new DateTime(2026,1,2,12,0,0)&&CarillonLog.Parse("01/02/2026 12:00:00 - current").Timestamp==new DateTime(2026,1,2,12,0,0));
            var report=EmailReports.Build(runtime);check("Report dates use U.S. slash format",report.Contains(DateTime.Now.ToString("MM/dd/yyyy"))&&report.Contains(DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy")));
            var missingPath=System.IO.Path.Combine(scratch,"Music","missing.mp3"); var missingReported=false;
            try { runtime.Play(new Song {Path=".\\Music\\missing.mp3"}); }
            catch(FileNotFoundException error) { missingReported=error.FileName==missingPath&&error.Message.Contains("Database import does not copy audio files"); }
            check("Missing music reports resolved path and recovery",missingReported&&!runtime.Player.Busy);
            var window=new MainWindow(runtime);
            try
            {
                var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                var begin=typeof(MainWindow).GetMethod("BeginEdit",flags)!; var save=typeof(MainWindow).GetMethod("Save",flags)!; var cancel=typeof(MainWindow).GetMethod("CancelClick",flags)!;
                foreach(var enabled in new[]{false,true})
                {
                    runtime.SetSchedule(enabled); window.SelectedSong=window.Songs.Single(s=>s.Path=="second.mp3"); var key=window.SelectedSong.Key;
                    begin.Invoke(window,[null,null]); check($"Edit pauses scheduling (previously {enabled})",!runtime.ScheduleEnabled);
                    window.EditingSong!.Repeats=2; cancel.Invoke(window,[null,null]);
                    check($"Cancel restores state and selection ({enabled})",runtime.ScheduleEnabled==enabled&&window.SelectedSong.Key==key&&window.EditingSong!.Repeats==1);
                    begin.Invoke(window,[null,null]); window.EditingSong!.Repeats=3;
                    check($"Save succeeds ({enabled})",(bool)save.Invoke(window,null)!);
                    window.Dispatcher.Invoke(()=>{},System.Windows.Threading.DispatcherPriority.ContextIdle);
                    var grid=(System.Windows.Controls.DataGrid)window.FindName("SongGrid");
                    check($"Reload keeps grid selection bound ({enabled})",ReferenceEquals(grid.SelectedItem,window.SelectedSong)&&System.Windows.Data.BindingOperations.IsDataBound(grid,System.Windows.Controls.DataGrid.SelectedItemProperty));
                    check($"Save restores state and stable selection ({enabled})",runtime.ScheduleEnabled==enabled&&window.SelectedSong!.Key==key&&window.EditingSong!.Repeats==3&&repository.Songs().Single(s=>s.Key==key).Repeats==3);
                    begin.Invoke(window,[null,null]); window.EditingSong!.Group="Changed Custom";
                    check($"Save changed identity succeeds ({enabled})",(bool)save.Invoke(window,null)!);
                    check($"Changed identity remains selected ({enabled})",runtime.ScheduleEnabled==enabled&&window.SelectedSong!.Path=="second.mp3"&&window.SelectedSong.Group=="Changed Custom");
                    begin.Invoke(window,[null,null]); window.EditingSong!.Repeats=1; window.EditingSong.Group="Custom"; save.Invoke(window,null);
                }

                clockNow = new DateTime(2025, 4, 13, 5, 7, 9);
                check("Uptime decomposes elapsed time into all seven units",
                    runtime.UptimeLabel == "1 year   1 month   2 weeks   1 day   4 hours   5 minutes   6 seconds");
                clockNow = new DateTime(2025, 2, 28, 1, 2, 3);
                check("Leap-day start observes the end-of-month anniversary",
                    runtime.UptimeLabel == "1 year   0 months   0 weeks   0 days   0 hours   0 minutes   0 seconds");
                clockNow = new DateTime(2028, 2, 29, 1, 2, 2);
                check("Uptime rolls over at the leap-year anniversary",
                    runtime.UptimeLabel == "3 years   11 months   4 weeks   2 days   23 hours   59 minutes   59 seconds");
                clockNow = new DateTime(2024, 2, 29, 1, 2, 2);
                check("Uptime remains nonnegative if the clock moves before startup",
                    runtime.UptimeLabel == "0 years   0 months   0 weeks   0 days   0 hours   0 minutes   0 seconds");
            }
            finally { window.Close(); }
        }
        finally { Directory.Delete(scratch,true); }
    }
}
