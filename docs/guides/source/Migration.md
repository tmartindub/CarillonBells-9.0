# Plan the migration

Carillon Bells 9.0 imports a SQLite database into a separate installation. Use the section for your starting version, **7.2.4 to 9.0** or **8.1 to 9.0**, then follow the shared import and validation instructions. Complete the migration on a test copy before connecting v9 to the production playback system.

## What to have ready

- The working 7.2.4 or 8.1 folder and the SQLite database it uses. In a standard portable installation, look for **databases\carillon.db**. Verify which file the running version uses before copying it.
- A fresh, writable folder for the complete v9 package. Use its own executable, runtime files and database. For example, use **D:\CarillonBells_9_0** for v9 on a portable drive.
- Your music folders, including the fallback and seasonal folders used by random playback. Their relative layout matters.
- A private backup destination with enough free space for a ZIP of the prior installation and its music. The legacy and v9 databases can contain email credentials.

## Choose the test mode and output

Use **Launch-Carillon-Evaluation.cmd** if it is included, or start **CarillonBells.exe** with **--evaluation**. Automatic email is suppressed in this mode. Scheduled playback and the Windows system volume controls are active. Keep the live bell amplifier disconnected during initial validation or select an appropriate test output.

# Prepare the two installations

1. Exit the old Carillon version and close any other application using its database. Make a ZIP of the working installation and verify that it contains the database before testing the migration.
2. Extract the entire v9 package to its own folder. The self-contained package includes its runtime.
3. Copy the music folders into the v9 folder with the same paths relative to the executable. If the old playlist refers to **.\Songs\Daily\Bell.mp3**, the test v9 installation needs **Songs\Daily\Bell.mp3** beside its executable.
4. Start v9 from the destination folder. If asked to create a blank database, accept so the app has a local **databases\carillon.db** to receive the import.
5. Clear **Schedule enabled** before reviewing the imported schedule. The administration dialog pauses an enabled scheduler while it is open, then restores its prior state when it closes.

## Keep each database separate

The executable folder determines where v9 reads and writes. An import replaces the destination v9 database. Your original installation and its database remain available for rollback. For an older database that stores a drive letter, v9 uses the folder layout after the drive root within its own installation. Confirm that all scheduled tracks and random folders resolve within that installation.

# 7.2.4 to 9.0

## Try a direct import into v9

1. Follow **Analyze the source database** with the closed 7.2.4 database or a verified copy. Expect **SQLite integrity: ok**. A standard 7.2.4 database reports **Legacy wide or missing** because its random music folders are stored as numbered columns in one row. In **Database schema**, look for **playlist**, **seasonalgroups**, and **PL_SETTINGS**.
2. Follow **Import and check the working copy** with the same file. On the v9 copy, the importer converts the old random directory values to seven slots, retains the original table as a backup, and creates twelve disabled weekly silence windows if the table was absent.
3. After import, compare the playlist and seasonal groups with 7.2.4. Confirm the fallback in slot 1, its dated overrides in slots 2–7, and the original liturgical or funeral silence settings under **System**. Then follow **Validate music and schedules**.

## Use the 8.1 converter if needed

If a healthy 7.2.4 database fails direct import, save the v9 report or record the error first. If you have the original 8.1 conversion utility, you can test an intermediate upgrade on a separate working database copy. Resolve any integrity errors against a verified backup before attempting either upgrade.

1. Close Carillon. From a separate 8.1 portable installation, start **CarillonDBUpgrade724To81.exe**. Click **Browse** and select the separate working copy of the 7.2.4 database. The utility upgrades the file you select in place.
2. Review the displayed format, check the confirmation box, and click **Back Up and Upgrade**. Keep the resulting backup and conversion report. Verify the converted playlist and settings in the separate 8.1 installation.
3. Close 8.1 and follow **8.1 to 9.0** below using the converted database as the source. Keep the original 7.2.4 ZIP for rollback.

# 8.1 to 9.0

1. Follow **Analyze the source database** with the closed 8.1 SQLite database or a verified copy. Expect **SQLite integrity: ok** and **8.1 row-based** for the normal seven-slot directory table.
2. Follow **Import and check the working copy** with the same file. Existing random directory slots and weekly silence rules carry over. Any missing standard slots are filled on the imported copy.
3. Compare the seven directory rows and twelve weekly silence rows with 8.1. Spot-check enabled rules, purposes, times and weekday selections. Then follow **Validate music and schedules**.

# Analyze the source database

1. Open **Database import** in the v9 sidebar. The **Database** tab shows the current local v9 database path. Confirm that it belongs to the intended destination.
2. Click **Analyze source database** and choose the old SQLite **.db** file. This operation opens the source read-only.
3. Review **SQLite integrity**. An **ok** result is the expected starting point. Investigate other messages or use a verified source backup before attempting import.
4. Review the directory schema line. **8.1 row-based** indicates that the random-directory table already uses individual slots. A legacy wide or missing directory table is handled on the imported copy. The label describes the directory structure.
5. Scroll through **Database schema** to review tables, views, indexes and triggers. The report includes SQLite schema definitions, which can reveal names from custom objects. **Save report** writes the report currently displayed to a text file. Save the analysis before importing if you want to retain both reports.

![This 8.1 example shows SQLite integrity as ok and identifies a row-based directory table. Scroll to inspect the remaining schema.](figures/analysis.png)

## Use the report to resolve source questions

The analysis is a structural and integrity check. It helps you see whether expected objects exist and how SQLite defines them. Verify playlist rows, file availability, group dates and report delivery through the practical checks later in this guide.

# Import and check the working copy

1. In the same **Database** tab, choose **Choose and import a database copy**. Resolve any pending administration edits if prompted.
2. Select the source **.db** that you analyzed. Choose the legacy database or a verified copy of it.
3. Wait for **Imported copy**. V9 snapshots the source into a temporary candidate, makes compatibility changes to the candidate and validates its core data. It backs up the current v9 database before putting the candidate in place.
4. Record the **Local ZIP backup** path in the import report. Use **Save report** if you want a separate migration record.
5. Review the reported playlist record count, random-directory slots, and weekly silence slots. Compare the playlist count against the source. Expect seven random slots and twelve weekly silence slots.

![The sample import brought over four playlist records. The report gives the ZIP path for restoring the previous v9 database.](figures/import.png)

## Compatibility changes you may see

V9 reuses a valid row-based random-directory table and converts older numbered directory columns into individual slots on the candidate. It retains the old wide table as a backup when it performs that conversion. It provides missing compatibility fields and fills missing standard random and silence slots. A database with multiple settings records requires review before the import can proceed.

Legacy time representations in known schedule columns are normalized to **HH:mm:ss**. A blank time stays absent. An explicit **00:00:00** is midnight. Import keeps custom and backup tables. The source analysis report provides a useful reference if you need to inspect an unfamiliar object after migration.

# Validate music and schedules

## Review the playlist and calendar

Open **Songs** and compare recognizable entries and groups with your starting version. Select representative daily and seasonal records and check their weekday flags, From and Until dates, play times and repeat counts. Use **Music groups** to review group dates and times. When scheduling resumes, group dates and the first group time can be applied to songs in matching seasonal groups.

Check the Windows time, date and audio device. Open **Today's schedule** to review scheduled times and the next occurrence. The remaining queue is live. Enable the schedule on the isolated test output when you are ready to check actual dispatch.

## Check random and manual playback

1. Open **Random music** and check slot 1, the fallback. A fresh v9 database uses **.\random_songs**. If the imported slot is blank, choose the intended fallback with **Browse**. Playback uses slot 1 outside the dated overrides.
2. Check slots 2 through 7 for complete annual date pairs and verify that each populated directory contains MP3 files at its top level. The first matching dated slot takes priority.
3. Use **Browse** on the corresponding row to select a folder inside the running v9 installation. Click **Save** to keep the change.
4. Use **Play** to check examples from the daily and seasonal libraries. Select a **Random Music Generator** playlist entry to check random playback on the date you are testing. Confirm the played file in **Log**.
5. Test a scheduled occurrence with the clock open and confirm it appears once in the playback log. Use only a disposable or deliberately approved test schedule.

## Check silence and reporting

Review all enabled **Silence windows**, including overnight rules and their starting weekdays. In **System**, review the liturgical and funeral settings separately. Look for the expected status in the **Silence window** card before production use.

Review **Email / reports** and use **Preview report** to save an HTML copy locally. Recheck the configured recipients, SMTP connection details and permission to send before using a live email test. The saved clock-hand style belongs to the v9 installation separately from the imported database.

# Move the portable copy and repeat checks

Close the test copy before moving it. Copy the complete working v9 folder, including its database, music, support files, and any saved appearance preferences. Launch the executable on the destination drive. Check that the next event, a manually played song and the fallback random folder resolve under that destination's folder. Open the **Database** tab to confirm the local database path.

Review the Windows output and volume again when connecting a different computer. Re-enable production scheduling only after checking the new instance, the active silence windows and the intended amplifier or output. Avoid running two active schedulers against the same output.

# Troubleshooting and rollback

## Resolve a failed migration

- **Source analysis fails**. Confirm you selected the real SQLite database and that another application has closed it. Check a verified backup if integrity errors persist.
- **The import reports multiple settings rows**. Preserve the source. Have the settings records reviewed in a private working copy before trying again.
- **A track is missing**. Check the full path named by v9, then copy the audio into the correct location inside the running installation. The database import carries records and paths. Music travels with the folder copy.
- **Random music is empty or unexpected**. Verify the first matching annual override for today and the fallback, then verify that the active folder contains MP3 files directly inside it.
- **The imported database opens but scheduling looks wrong**. Check today's date, enabled flags, inclusive play dates, actual weekday flags, group synchronization and active silence rules. Compare with the preserved v9 import report and the old working schedule.

## Roll back the v9 database

1. Disable scheduling and close v9.
2. Preserve a ZIP of the current v9 database if it contains new work you may need.
3. Check that the pre-import ZIP named in the import report opens and contains **carillon.db**. Restore that file to the intended v9 folder as **databases\carillon.db** while v9 is closed. Keep the saved ZIP for reference.
4. Reopen v9 in evaluation mode and review the playlist, settings and schedule before reconnecting production audio.

If you need to resume 7.2.4 or 8.1 immediately, close v9 and launch the preserved installation from its own directory. Confirm the original database, schedule and audio output before returning to service.
