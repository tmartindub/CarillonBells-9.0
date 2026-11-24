using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Carillon;
public partial class AdministrationWindow : Window
{
    private readonly CarillonRuntime runtime;
    public DataTable Groups { get; private set; } = new();
    public DataTable RandomDirectories { get; private set; } = new();
    public DataTable Silence { get; private set; } = new();
    private DataTable settings=new();
    private string originalPassword="";
    private bool loading;
    public bool RequiresScheduleResume { get; private set; }
    public AdministrationWindow(CarillonRuntime runtime,string page)
    {
        this.runtime=runtime; RequiresScheduleResume=page is not ("Themes" or "ClockHands"); loading=true; InitializeComponent(); LoadData();
        loading=true;ThemeList.SelectedItem=((CarillonPalette[])FindResource("ThemeCatalog")).FirstOrDefault(p=>p.Name==ThemeManager.CurrentPalette.Name);CustomBackground.Text=ThemeManager.CurrentPalette.Canvas;CustomText.Text=ThemeManager.CurrentPalette.Ink;
        Pages.SelectedItem=Pages.Items.OfType<TabItem>().FirstOrDefault(t=>t.Tag?.ToString()==page); loading=false;
    }
    private void LoadData()
    {
        loading=true;
        Groups=runtime.Repository.Table("seasonalgroups"); RandomDirectories=runtime.Repository.Table("PL_RAND_DIR_ROTATION"); Silence=runtime.Repository.Table("silence_schedule"); settings=runtime.Repository.Table("PL_SETTINGS");
        // Format only the editable view; repository writes keep ISO dates.
        foreach(DataRow row in Groups.Rows) row["PLAY_TIME"]=(object?)DataFormats.Time(row["PLAY_TIME"])?.ToString("HH:mm:ss",System.Globalization.CultureInfo.InvariantCulture)??DBNull.Value;
        foreach(var table in new[]{Groups,RandomDirectories,settings})
        {
            foreach(DataColumn column in table.Columns)
            {
                var field=column.ColumnName.ToLowerInvariant();
                if(field is not ("play_date_from" or "play_date_to" or "frm_date" or "to_date" or "ash_wed_begin" or "ash_wed_end" or "holy_thu_begin" or "holy_sat_end" or "all_souls_day" or "other_silence_begin_date" or "other_silence_end_date")) continue;
                foreach(DataRow row in table.Rows) row[column]=(object?)DataFormats.Date(row[column])?.ToString(table==RandomDirectories?"M/d":"MM/dd/yyyy",System.Globalization.CultureInfo.InvariantCulture)??DBNull.Value;
            }
            table.AcceptChanges();
        }
        DataContext=null; DataContext=this;
        SystemFieldsPanel.DataContext=settings.DefaultView[0]; EmailFieldsPanel.DataContext=settings.DefaultView[0];
        originalPassword=CarillonRepository.Text(settings.Rows[0],"email_password"); EmailPassword.Password=originalPassword; VisiblePassword.Text=originalPassword;
        DatabaseLabel.Text="Current local database: "+runtime.Repository.DatabasePath;
        ClockHandPicker.SelectedValue = runtime.ClockHandStyleId;
        loading=false;
    }
    private void CommitEditors()
    {
        foreach(var grid in new[]{GroupsGrid,RandomGrid,SilenceGrid}) { if(!grid.CommitEdit(DataGridEditingUnit.Cell,true)||!grid.CommitEdit(DataGridEditingUnit.Row,true)) throw new ArgumentException("Finish or cancel the invalid grid edit first."); }
        settings.DefaultView[0].EndEdit();
    }
    private string Password=>ShowPassword.IsChecked==true?VisiblePassword.Text:EmailPassword.Password;
    private bool SettingsDirty=>Groups.GetChanges() is not null||RandomDirectories.GetChanges() is not null||Silence.GetChanges() is not null||settings.GetChanges() is not null||Password!=originalPassword;
    private bool ClockHandsDirty=>ClockHandPicker.SelectedItem is ClockHandStyle style && style.Id != runtime.ClockHandStyleId;
    private bool Dirty=>SettingsDirty || ClockHandsDirty;
    private bool Save()
    {
        try
        {
            CommitEditors();
            if (SettingsDirty)
            {
                Validate();
                runtime.Repository.SaveTable(Groups,"SEASONALGROUP"); runtime.Repository.SaveTable(RandomDirectories,"slot_no"); runtime.Repository.SaveTable(Silence,"silence_id");
                var row=settings.Rows[0]; var values=settings.Columns.Cast<DataColumn>().ToDictionary(c=>c.ColumnName,c=>row[c],StringComparer.OrdinalIgnoreCase); values["email_password"]=Password;
                var changed=CarillonRepository.SystemFields.Concat(CarillonRepository.EmailFields).Where(f=>!Equals(row[f],row[f,DataRowVersion.Original])||f=="email_password"&&Password!=originalPassword).ToArray();
                if(changed.Length>0) runtime.Repository.SaveSettings(values,changed);
                runtime.Repository.SyncGroups(); runtime.Reload();
            }
            if (ClockHandPicker.SelectedItem is ClockHandStyle style && style.Id != runtime.ClockHandStyleId)
            {
                runtime.SaveClockHandStyle(style.Id);
                Application.Current.Resources["SelectedClockHands"] = style;
            }
            LoadData(); ResultLabel.Text="Saved"; return true;
        }
        catch(Exception error) { MessageBox.Show(this,error.Message,"Cannot save settings"); return false; }
    }
    private void Validate()
    {
        var row=settings.Rows[0];
        foreach(var field in CarillonRepository.SystemFields.Where(f=>f.Contains("date")||f is "ash_wed_begin" or "ash_wed_end" or "holy_thu_begin" or "holy_sat_end" or "all_souls_day")) DataFormats.Date(row[field]);
        foreach(var field in new[]{"LOGGING_STATUS","silence_enabled","email_enabled"}) if(CarillonRepository.Int(row,field) is <0 or >1) throw new ArgumentException(field+" must be 0 or 1.");
        var start=DataFormats.Date(row["other_silence_begin_date"]); var end=DataFormats.Date(row["other_silence_end_date"]); var startTime=DataFormats.Time(row["other_silence_begin_time"]); var endTime=DataFormats.Time(row["other_silence_end_time"]);
        if((startTime is not null||endTime is not null) && (start is null||end is null||startTime is null||endTime is null||start+startTime.Value.ToTimeSpan()>=end+endTime.Value.ToTimeSpan())) throw new ArgumentException("Funeral silence requires complete begin/end dates and times, with end after begin.");
        if(DataFormats.Date(row["holy_thu_begin"])>DataFormats.Date(row["holy_sat_end"])) throw new ArgumentException("Holy Thursday must be before Holy Saturday.");
        foreach(DataRow window in Silence.Rows)
        {
            var from=DataFormats.Time(window["start_time"]); var to=DataFormats.Time(window["end_time"]);
            if(CarillonRepository.Int(window,"enabled")!=0&&(from is null||to is null)) throw new ArgumentException("An enabled silence window requires start and end times.");
        }
        foreach(DataRow group in Groups.Rows)
        {
            if(group.RowState==DataRowState.Deleted) continue;
            if(string.IsNullOrWhiteSpace(CarillonRepository.Text(group,"SEASONALGROUP"))) throw new ArgumentException("A group name is required.");
            if(DataFormats.Date(group["PLAY_DATE_FROM"])>DataFormats.Date(group["PLAY_DATE_TO"])) throw new ArgumentException("Group From must be before Until."); DataFormats.Time(group["PLAY_TIME"]);
        }
        foreach(DataRow rotation in RandomDirectories.Rows)
        {
            var from=DataFormats.AnnualDate(rotation["frm_date"]); var to=DataFormats.AnnualDate(rotation["to_date"]);
            if(CarillonRepository.Int(rotation,"slot_no")>1&&((from is null)!=(to is null))) throw new ArgumentException("An annual override requires both From and Until dates.");
        }
    }
    private void SaveClick(object sender,RoutedEventArgs e)=>Save();
    private void CancelClick(object sender,RoutedEventArgs e) { LoadData(); ResultLabel.Text="Changes cancelled"; }
    private bool ConfirmPending()
    {
        try { CommitEditors(); } catch(Exception error) { MessageBox.Show(this,error.Message); return false; }
        if(!Dirty) return true;
        var answer=MessageBox.Show(this,"Save administration changes before closing?","Pending changes",MessageBoxButton.YesNoCancel);
        return answer==MessageBoxResult.Yes?Save():answer==MessageBoxResult.No;
    }
    private void WindowClosing(object? sender,CancelEventArgs e) { if(!ConfirmPending()) e.Cancel=true; }
    private void CloseClick(object sender,RoutedEventArgs e)=>Close();
    private void NavigateClick(object sender,RoutedEventArgs e)
    {
        var page=(Pages.SelectedItem as TabItem)?.Tag?.ToString();var grid=page switch {"Groups"=>GroupsGrid,"Random"=>RandomGrid,"Silence"=>SilenceGrid,_=>null};if(grid is null)return;
        try {CommitEditors();var count=grid.Items.Count-(grid.CanUserAddRows?1:0);if(count==0)return;var index=Math.Max(0,grid.SelectedIndex);grid.SelectedIndex=((Button)sender).Tag?.ToString() switch {"First"=>0,"Last"=>count-1,"Previous"=>Math.Max(0,index-1),_=>Math.Min(count-1,index+1)};grid.ScrollIntoView(grid.SelectedItem);}catch(Exception error){MessageBox.Show(this,error.Message);}
    }
    private void ClearSilenceWindowClick(object sender, RoutedEventArgs e)
    {
        try
        {
            CommitEditors();
            if (SilenceGrid.SelectedItems.Count != 1 || SilenceGrid.SelectedItem is not DataRowView row)
                return;

            row["enabled"] = 0;
            row["Purpose"] = $"Silence Window {row["silence_id"]}";
            row["start_time"] = DBNull.Value;
            row["end_time"] = DBNull.Value;
            foreach (var day in Song.DatabaseDays)
                row[day] = 0;
            ResultLabel.Text = $"Window {row["silence_id"]} cleared. Click Save to apply.";
        }
        catch (Exception error) { MessageBox.Show(this, error.Message, "Cannot clear silence window"); }
    }
    private void AddGroupClick(object sender,RoutedEventArgs e) {Pages.SelectedItem=Pages.Items.OfType<TabItem>().First(t=>t.Tag?.ToString()=="Groups");var row=Groups.NewRow();row["SEASONALGROUP"]="New Group "+DateTime.Now.ToString("HHmmss");Groups.Rows.Add(row);GroupsGrid.SelectedIndex=Groups.Rows.Count-1;GroupsGrid.ScrollIntoView(GroupsGrid.SelectedItem);}
    private void DeleteGroupClick(object sender,RoutedEventArgs e) {if(GroupsGrid.SelectedItem is DataRowView row&&MessageBox.Show(this,"Delete the selected group? Playlist songs are retained.","Delete group",MessageBoxButton.YesNo)==MessageBoxResult.Yes)row.Delete();}
    private void PageChanged(object sender,SelectionChangedEventArgs e)
    {
        if(loading||e.Source!=Pages) return; ResultLabel.Text="";
        var page=(Pages.SelectedItem as TabItem)?.Tag?.ToString();
        if(page is "Themes" or "ClockHands") return;
        var reason=page switch {"Groups"=>"Groups maintenance","Random"=>"random music maintenance","Silence"=>"silence-window maintenance","Email"=>"email/report maintenance","Database"=>"database maintenance",_=>"System maintenance"};
        if(!RequiresScheduleResume) {runtime.SetSchedule(false,reason);RequiresScheduleResume=true;}
        else runtime.Log.Append("Schedule remains disabled - "+reason);
    }
    private void RecalculateGroupsClick(object sender,RoutedEventArgs e)
    {
        try { CommitEditors(); foreach(DataRow row in Groups.Rows) if(row.RowState!=DataRowState.Deleted&&CalendarRules.GroupDates(CarillonRepository.Text(row,"SEASONALGROUP"),DateTime.Today) is {} dates) {row["PLAY_DATE_FROM"]=dates.From.ToString("MM/dd/yyyy");row["PLAY_DATE_TO"]=dates.To.ToString("MM/dd/yyyy");} ResultLabel.Text="Dates recalculated; click Save"; } catch(Exception error) { MessageBox.Show(this,error.Message); }
    }
    private void RecalculateSettingsClick(object sender,RoutedEventArgs e)
    {
        var easter=CalendarRules.Easter(DateTime.Today.Year); if(easter<DateTime.Today) easter=CalendarRules.Easter(DateTime.Today.Year+1);
        var row=settings.Rows[0]; row["ash_wed_begin"]=easter.AddDays(-46).ToString("MM/dd/yyyy"); row["holy_thu_begin"]=easter.AddDays(-3).ToString("MM/dd/yyyy"); row["holy_sat_end"]=easter.AddDays(-1).ToString("MM/dd/yyyy");
        var souls=new DateTime(DateTime.Today.Year,11,2); if(souls<DateTime.Today) souls=souls.AddYears(1); row["all_souls_day"]=souls.ToString("MM/dd/yyyy"); ResultLabel.Text="Dates recalculated; click Save";
    }
    private void ClearFuneralClick(object sender,RoutedEventArgs e) { foreach(var field in new[]{"other_silence_begin_date","other_silence_end_date","other_silence_begin_time","other_silence_end_time"}) settings.Rows[0][field]=DBNull.Value; }
    private void BrowseRandomClick(object sender,RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: DataRowView row }) return;
        try
        {
            CommitEditors();
            var dialog = new OpenFolderDialog
            {
                Title = $"Choose a random music folder for slot {row["slot_no"]}",
                InitialDirectory = runtime.BaseDirectory
            };
            var current = CarillonRepository.Text(row.Row,"rand_music_dir");
            if (!string.IsNullOrWhiteSpace(current))
            {
                try
                {
                    var currentFolder = DataFormats.PortablePath(runtime.BaseDirectory,current);
                    if (Directory.Exists(currentFolder)) dialog.InitialDirectory = currentFolder;
                }
                catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
                {
                    // An invalid typed path can be replaced by browsing from the application folder.
                }
            }

            if (dialog.ShowDialog(this) != true) return;
            var relative = Path.GetRelativePath(runtime.BaseDirectory,dialog.FolderName);
            if (!DataFormats.PortablePath(runtime.BaseDirectory,relative).Equals(Path.GetFullPath(dialog.FolderName),StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Copy the music directory into this portable application folder first.");
            row["rand_music_dir"] = ".\\" + relative;
        }
        catch(Exception error) { MessageBox.Show(this,error.Message,"Portable music directory"); }
    }
    private void ShowPasswordClick(object sender,RoutedEventArgs e)
    {
        var show=ShowPassword.IsChecked==true;
        if(show) VisiblePassword.Text=EmailPassword.Password; else EmailPassword.Password=VisiblePassword.Text;
        VisiblePassword.Visibility=show?Visibility.Visible:Visibility.Collapsed; EmailPassword.Visibility=show?Visibility.Collapsed:Visibility.Visible;
    }
    private async void TestEmailClick(object sender,RoutedEventArgs e)=>await SendTest(false);
    private async void TestMorningClick(object sender,RoutedEventArgs e)=>await SendTest(true);
    private async Task SendTest(bool report)
    {
        if(!Save()) return;
        var recipients=runtime.Setting(report?"morning_report_recipients":"test_recipient");
        if(MessageBox.Show(this,"Send a real "+(report?"morning report":"test email")+" to the configured "+(report?"morning-report recipients":"test recipient")+"?","Confirm email",MessageBoxButton.YesNo)!=MessageBoxResult.Yes) return;
        try { await EmailReports.SendAsync(runtime,App.ProductTitle+(report?" - Morning Health Report":" - Test Message"),report?EmailReports.Build(runtime):"<p>The email test completed successfully.</p>",recipients,recipientLabel:report?"Morning report recipients":"Test recipient"); ResultLabel.Text="Message sent"; }
        catch(Exception error) { runtime.Fail("Email failed: "+error.Message); MessageBox.Show(this,error.Message,"Email failed"); }
    }
    private void PreviewReportClick(object sender,RoutedEventArgs e)
    {
        try { var dialog=new SaveFileDialog {Filter="HTML report|*.html",FileName="Carillon-health-report.html"}; if(dialog.ShowDialog(this)==true) { File.WriteAllText(dialog.FileName,EmailReports.Build(runtime)); ResultLabel.Text="Report saved"; } } catch(Exception error) { MessageBox.Show(this,error.Message); }
    }
    private void ThemeChanged(object sender,SelectionChangedEventArgs e) { if(loading||ThemeList.SelectedItem is not CarillonPalette theme) return; try { ThemeManager.Apply(theme,runtime.Repository); } catch(Exception error) { MessageBox.Show(this,error.Message); } }
    private void CustomThemeClick(object sender,RoutedEventArgs e) { try { ThemeManager.Apply(new CarillonPalette {Name="Current: Custom Saved Colors",Canvas=CustomBackground.Text,Ink=CustomText.Text,Surface=CustomBackground.Text},runtime.Repository); ResultLabel.Text="Custom colors saved"; } catch(Exception error) { MessageBox.Show(this,error.Message); } }
    private void ImportClick(object sender,RoutedEventArgs e)
    {
        if(!ConfirmPending()) return;
        var dialog=new OpenFileDialog {Title="Import a copy of a Carillon SQLite database",Filter="SQLite database|*.db"}; if(dialog.ShowDialog(this)!=true) return;
        if(Path.GetFullPath(dialog.FileName).Equals(runtime.Repository.DatabasePath,StringComparison.OrdinalIgnoreCase)) { MessageBox.Show(this,"This is already the local database."); return; }
        var temporary=runtime.Repository.DatabasePath+".import"; string? backup=null;
        try {
            using(var source=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=dialog.FileName,Mode=SqliteOpenMode.ReadOnly,Pooling=false}.ToString()))
            using(var target=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=temporary,Pooling=false}.ToString())) { source.Open(); target.Open(); source.BackupDatabase(target); }
            var candidate=new CarillonRepository(temporary); candidate.EnsureCompatibleSchema();var normalized=candidate.NormalizeLegacyTimes(); candidate.Songs(); candidate.Silence(); candidate.Settings();
            backup=runtime.Repository.BackupZip(); runtime.Player.Stop(); File.Move(temporary,runtime.Repository.DatabasePath,true); runtime.Repository.EnsureRuntimeTables(); runtime.Reload(); LoadData(); ResultLabel.Text="Imported copy; source unchanged";
            MigrationReport.Text=$"Import completed {DateTime.Now:MM/dd/yyyy h:mm:ss tt}\nSource opened read-only and left unchanged.\nLocal ZIP backup: {backup}\nNormalized time values in working copy: {normalized}\nPlaylist records: {runtime.Songs.Count}\nRandom directory slots: {RandomDirectories.Rows.Count}\nWeekly silence slots: {Silence.Rows.Count}\nCustom and backup tables preserved.";
        } catch(Exception error) { if(backup is not null) { using var zip=ZipFile.OpenRead(backup); zip.GetEntry("carillon.db")!.ExtractToFile(runtime.Repository.DatabasePath,true); } MessageBox.Show(this,error.Message,"Import failed"); }
        finally { if(File.Exists(temporary)) File.Delete(temporary); }
    }
    private void AnalyzeDatabaseClick(object sender,RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title="Analyze a Carillon database read-only", Filter="SQLite database|*.db" };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = dialog.FileName, Mode = SqliteOpenMode.ReadOnly, Pooling = false
            }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            var report = new StringBuilder();
            report.AppendLine($"Read-only analysis {DateTime.Now:MM/dd/yyyy h:mm:ss tt}");

            command.CommandText = "PRAGMA quick_check";
            using (var reader = command.ExecuteReader())
                while (reader.Read()) report.AppendLine($"SQLite integrity: {reader.GetString(0)}");

            command.CommandText = "PRAGMA table_info(PL_RAND_DIR_ROTATION)";
            var rowBased = false;
            using (var reader = command.ExecuteReader())
                while (reader.Read()) rowBased |= reader.GetString(1).Equals("slot_no", StringComparison.OrdinalIgnoreCase);
            report.AppendLine($"Directory schema: {(rowBased ? "8.1 row-based" : "Legacy wide or missing; conversion will operate on an imported copy")}");
            report.AppendLine("No SQL writes were made to the selected source.");
            report.AppendLine();
            report.AppendLine("Database schema");

            command.CommandText = """
                SELECT type, name, tbl_name, sql FROM main.sqlite_schema
                ORDER BY CASE type WHEN 'table' THEN 0 WHEN 'view' THEN 1 WHEN 'index' THEN 2 ELSE 3 END,
                    name COLLATE NOCASE, name
                """;
            using (var reader = command.ExecuteReader())
            {
                if (!reader.HasRows) report.AppendLine("No schema objects found.");
                while (reader.Read())
                {
                    var objectType = reader.GetString(0);
                    var objectName = reader.GetString(1);
                    var tableName = reader.GetString(2);
                    report.AppendLine();
                    report.Append($"{objectType.ToUpperInvariant()} {objectName}");
                    if (objectType is "index" or "trigger") report.Append($" on {tableName}");
                    report.AppendLine();
                    report.AppendLine(reader.IsDBNull(3) ? "Created automatically by SQLite. See the table definition." : reader.GetString(3));
                }
            }
            MigrationReport.Text = report.ToString();
        }
        catch (Exception error) { MigrationReport.Text = "Analysis failed: " + error.Message; }
        MigrationReport.Select(0, 0);
        MigrationReport.ScrollToHome();
    }
    private void SaveMigrationReportClick(object sender,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(MigrationReport.Text))return;var dialog=new SaveFileDialog{Filter="Text report|*.txt",FileName="Carillon-database-conversion-report.txt"};if(dialog.ShowDialog(this)==true)try{File.WriteAllText(dialog.FileName,MigrationReport.Text);}catch(Exception error){MessageBox.Show(this,error.Message);}}
}
