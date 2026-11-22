CREATE TABLE PL_RAND_DIR_ROTATION (slot_no INTEGER PRIMARY KEY CHECK (slot_no BETWEEN 1 AND 7), rand_music_dir TEXT CHECK (LENGTH(rand_music_dir) <= 150), frm_date DATE, to_date DATE);
CREATE TABLE "PL_RAND_DIR_ROTATION_WIDE_BACKUP" ( id INTEGER PRIMARY KEY CHECK (id = 1), rand_music_dir1 TEXT CHECK (LENGTH(rand_music_dir1) <= 150), frm_date1 DATE, to_date1 DATE, rand_music_dir2 TEXT CHECK (LENGTH(rand_music_dir2) <= 150), frm_date2 DATE, to_date2 DATE, rand_music_dir3 TEXT CHECK (LENGTH(rand_music_dir3) <= 150), frm_date3 DATE, to_date3 DATE, rand_music_dir4 TEXT CHECK (LENGTH(rand_music_dir4) <= 150), frm_date4 DATE, to_date4 DATE, rand_music_dir5 TEXT CHECK (LENGTH(rand_music_dir5) <= 150), frm_date5 DATE, to_date5 DATE, rand_music_dir6 TEXT CHECK (LENGTH(rand_music_dir6) <= 150), frm_date6 DATE, to_date6 DATE, rand_music_dir7 TEXT CHECK (LENGTH(rand_music_dir7) <= 150), frm_date7 DATE, to_date7 DATE );
CREATE TABLE "PL_SETTINGS" (
    ORGANIZATION_NAME TEXT CHECK (LENGTH(ORGANIZATION_NAME) <= 100),
    RAND_SONGS_DIRECTORY TEXT CHECK (LENGTH(RAND_SONGS_DIRECTORY) <= 150)
, LOGGING_STATUS INTEGER DEFAULT 0 CHECK (LOGGING_STATUS IN (0, 1)), form_bgcolor TEXT, form_fontcolor TEXT, all_souls_day DATE, other_silence_begin_date DATE, other_silence_end_date DATE, silence_enabled INTEGER DEFAULT 1, ash_wed_begin DATE, ash_wed_end DATE, holy_thu_begin DATE, holy_sat_end DATE, other_silence_begin_time TIME, other_silence_end_time TIME, email_enabled INTEGER DEFAULT 0, email_recipients TEXT, email_user TEXT, email_password TEXT, smtp_host TEXT, smtp_port INTEGER, from_address TEXT, test_recipient TEXT, morning_report_recipients TEXT, settings_id INTEGER NOT NULL DEFAULT 1);
CREATE TABLE playlist (
    playlist_name TEXT NOT NULL,
    song_name TEXT NOT NULL,
    season TEXT,
    play_date_from DATE,
    play_date_to DATE,
    scheduled_time1 TIME,
    scheduled_time2 TIME,
    scheduled_time3 TIME,
    scheduled_time4 TIME,
    scheduled_time5 TIME,
    scheduled_time6 TIME,
    scheduled_time7 TIME,
    scheduled_time8 TIME,
    scheduled_time9 TIME,
    scheduled_time10 TIME,
    scheduled_time11 TIME,
    scheduled_time12 TIME,
    play_monday INTEGER DEFAULT 1,
    play_tuesday INTEGER DEFAULT 1,
    play_wednesday INTEGER DEFAULT 1,
    play_thursday INTEGER DEFAULT 1,
    play_friday INTEGER DEFAULT 1,
    play_saturday INTEGER DEFAULT 1,
    play_sunday INTEGER DEFAULT 1,
    num_times_to_play INTEGER DEFAULT 1,
    song_duration TEXT,
    PRIMARY KEY (playlist_name, song_name)
);
CREATE TABLE seasonalgroups (
    SEASONALGROUP TEXT NOT NULL PRIMARY KEY,
    PLAY_DATE_FROM DATE,
    PLAY_DATE_TO DATE,
    PLAY_TIME TIME
);
CREATE TABLE silence_schedule (silence_id INTEGER NOT NULL PRIMARY KEY, enabled INTEGER NOT NULL DEFAULT 0, Purpose TEXT, start_time TEXT, end_time TEXT, monday INTEGER NOT NULL DEFAULT 0, tuesday INTEGER NOT NULL DEFAULT 0, wednesday INTEGER NOT NULL DEFAULT 0, thursday INTEGER NOT NULL DEFAULT 0, friday INTEGER NOT NULL DEFAULT 0, saturday INTEGER NOT NULL DEFAULT 0, sunday INTEGER NOT NULL DEFAULT 0);
CREATE UNIQUE INDEX idx_pl_settings_settings_id ON PL_SETTINGS(settings_id);
INSERT INTO PL_SETTINGS(settings_id,ORGANIZATION_NAME,LOGGING_STATUS,silence_enabled,email_enabled) VALUES(1,'Carillon Bells',1,0,0);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(1);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(2);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(3);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(4);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(5);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(6);
INSERT INTO PL_RAND_DIR_ROTATION(slot_no) VALUES(7);
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(1,'Silence Window 1');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(2,'Silence Window 2');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(3,'Silence Window 3');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(4,'Silence Window 4');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(5,'Silence Window 5');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(6,'Silence Window 6');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(7,'Silence Window 7');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(8,'Silence Window 8');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(9,'Silence Window 9');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(10,'Silence Window 10');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(11,'Silence Window 11');
INSERT INTO silence_schedule(silence_id,Purpose) VALUES(12,'Silence Window 12');
