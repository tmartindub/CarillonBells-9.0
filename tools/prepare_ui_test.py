from pathlib import Path
import sqlite3
root=Path(__file__).resolve().parents[1]
stage=root/'export/staging-win-x64'
(stage/'databases').mkdir(exist_ok=True)
target=stage/'databases/carillon.db'
if target.exists(): raise RuntimeError('UI test database already exists; refusing to overwrite it.')
connection=sqlite3.connect(target)
connection.executescript((root/'samples/empty-schema.sql').read_text())
connection.execute("INSERT INTO playlist(playlist_name,song_name,season,play_date_from,play_date_to,num_times_to_play,song_duration,play_monday,play_tuesday,play_wednesday,play_thursday,play_friday,play_saturday,play_sunday,scheduled_time1) VALUES('Daily List','validation.mp3','Daily List','2026-01-01','2027-12-31',1,'1:30',1,1,1,1,1,1,1,'12:00:00')")
connection.commit();connection.close()
print('Created disposable sanitized UI database.')
