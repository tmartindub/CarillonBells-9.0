"""Author static design-time XAML fields and sanitized schema assets."""
from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
schema=json.loads((root/'docs/guides/database-schema.json').read_text())
sql='\n'.join(item['sql']+';' for item in schema if item['type']=='table')+'\n'
sql+='\n'.join(item['sql']+';' for item in schema if item['type']=='index')+'\n'
sql+="INSERT INTO PL_SETTINGS(settings_id,ORGANIZATION_NAME,LOGGING_STATUS,silence_enabled,email_enabled) VALUES(1,'Carillon Bells',1,0,0);\n"
for i in range(1,8): sql+=f'INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES({i});\n'
for i in range(1,13): sql+=f"INSERT INTO silence_schedule(silence_id,Purpose) VALUES({i},'Silence Window {i}');\n"
(root/'samples/empty-schema.sql').write_text(sql,encoding='utf-8')
path=root/'AdministrationWindow.xaml'
text=path.read_text(encoding='utf-8')
system=[('Organization name','ORGANIZATION_NAME'),('Fallback random directory (legacy)','RAND_SONGS_DIRECTORY'),('Playback logging','LOGGING_STATUS'),('Enable liturgical and funeral silence','silence_enabled'),('Ash Wednesday (yyyy-MM-dd)','ash_wed_begin'),('Ash Wednesday end (legacy compatibility)','ash_wed_end'),('Holy Thursday (yyyy-MM-dd)','holy_thu_begin'),('Holy Saturday (yyyy-MM-dd)','holy_sat_end'),('All Souls Day (yyyy-MM-dd)','all_souls_day'),('Funeral begin date (optional)','other_silence_begin_date'),('Funeral begin time','other_silence_begin_time'),('Funeral end date (optional)','other_silence_end_date'),('Funeral end time','other_silence_end_time')]
email=[('Enable email','email_enabled'),('SMTP host','smtp_host'),('SMTP port (STARTTLS)','smtp_port'),('Username','email_user'),('From address','from_address'),('Alert recipients','email_recipients'),('Test recipient','test_recipient'),('Morning report recipients','morning_report_recipients')]
def fields(items):
    result=''
    for label,key in items:
        result+=f'<TextBlock Text="{label}" Margin="0,9,0,4"/>'
        if key in ('LOGGING_STATUS','silence_enabled','email_enabled'):
            result+=f'<CheckBox IsChecked="{{Binding [{key}], Converter={{StaticResource IntBool}}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}"/>'
        else:
            result+=f'<TextBox Text="{{Binding [{key}], Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}}" MaxWidth="700" HorizontalAlignment="Left" Width="700"/>'
    return result
text=text.replace('<StackPanel x:Name="SystemFieldsPanel" Margin="22"/>','<StackPanel x:Name="SystemFieldsPanel" Margin="22">'+fields(system)+'<WrapPanel Margin="0,16"><Button Content="Recalculate Easter / All Souls" Click="RecalculateSettingsClick"/><Button Content="Clear funeral dates" Click="ClearFuneralClick" Margin="12,0"/></WrapPanel></StackPanel>')
text=text.replace('<StackPanel x:Name="EmailFieldsPanel"/>','<StackPanel x:Name="EmailFieldsPanel">'+fields(email)+'</StackPanel>')
path.write_text(text,encoding='utf-8')
print('Static XAML settings fields and sanitized empty database schema prepared.')
