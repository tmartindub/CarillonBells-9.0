# Prepare the USB upgrade

Upgrade Westminster Chimes and Carillon Bells by putting the v9 executable beside the existing database and music on the USB. In Windows **Settings**, open **System**, then **About**, and check **System type** on the PC that will run the app. Choose **win-x86** from the **32-bit** release folder for 32-bit Windows or **win-x64** from **64-bit** for 64-bit Windows on an x64 PC.

Choose **CarillonBells_9_0_USB_Upgrade_win-x86.zip** or **CarillonBells_9_0_USB_Upgrade_win-x64.zip** to match Windows. Both contain **CarillonBells.exe** and **Documentation**. If you have the matching **Upgrade Package Setup**, run it on the PC to save the ZIP in your chosen folder.

1. Close version 7.2.4 or 8.1 and any other program using its database.
2. In File Explorer, locate the folder on the USB that contains the working **databases** folder. If **databases** is directly on the drive, use the drive root, such as **D:\**.
3. Right-click the USB Upgrade ZIP and choose **Extract All**. Extract to the folder you identified. Check that **CarillonBells.exe** and **databases** are side by side. Then follow the instructions below for your current version.

For a new USB, use the matching **USB New Install** package and follow the **Quick Start Guide**. It supplies the starter database and chimes.

# Upgrade from version 7.2.4

1. Open **CarillonBells.exe** directly from the USB installation folder.
2. In the **Upgrade from 7.2.4** confirmation, click **Yes**. The app creates and verifies a dated backup ZIP in **databases**, then upgrades the working database. The ZIP name starts with **carillon_before_upgrade_** and holds the original **carillon.db**.
3. When v9 opens, scheduling is paused and automatic email is suppressed for this review session. Open **Log** for the backup path. Check the songs, groups and settings before using the live schedule.

The conversion prepares seven random-directory slots, keeps the original wide table, and fills missing compatibility fields and weekly silence slots. It preserves the stored schedule times. The music stays in its existing folders. Compare the fallback, seasonal overrides, playlist and group schedules after the upgrade.

# Upgrade from version 8.1

Open **CarillonBells.exe** directly from the USB installation folder. Version 9.0 starts with your existing database and music. If the dispatch table is missing, the app saves a dated backup ZIP in **databases** before adding it. The dispatch table prevents a scheduled play from repeating after a normal restart.

A normal start enables scheduling. Set a suitable Windows output and level before starting, then clear **Schedule enabled** while you review the installation. Compare the playlist, seven random-directory slots and twelve weekly silence rows with your 8.1 setup.

# Verify the upgraded USB

1. In v9, open **Database import** and check that **Current local database** points to **databases\carillon.db** on the USB you upgraded. Verify that several familiar songs and groups appear under **Songs**.
2. Check **Music groups** and **Today's schedule**. Compare representative weekday flags, date ranges and play times. Check the PC clock and the **Next song** card.
3. Use **Play** to check a familiar recording through a test output. In **Random music**, confirm the intended slot 1 fallback and the dated folders in slots 2–7. For a setup that uses **.\random_songs**, verify that folder is present beside the executable and contains MP3 files. If you browse to a corrected folder, click **Save**.
4. Review **Silence windows** and the liturgical and funeral settings under **System**. In **Email / reports**, use **Preview report** to see the report locally and confirm the mail settings before normal use.
5. When ready, use **Schedule enabled** to verify a scheduled event with your test output and check **Log** for the recorded play. To finish a 7.2.4 review session, close v9 and start **CarillonBells.exe** directly from the USB for normal operation. Confirm that the old version is closed and the desired output is connected.

For a second portable device, close the app and copy the complete working installation, including the executable, database and existing music folders. Run the copied executable and check the local database path and playback. The folders move together even when Windows assigns a different drive letter.

# Recover or roll back

If the database is in use, close all instances of the app and any other database tools, then start v9 from the USB again. If startup reports a missing database, check that the executable is beside the **databases** folder of the USB you intended to upgrade. If a recording cannot play, check the original music folder and confirm the playlist path or random directory.

Use these steps to return the upgraded USB to the old version.

1. Close v9 and the old version. Preserve any new v9 changes you may need in a separate ZIP.
2. Find the dated ZIP starting with **carillon_before_upgrade_** in **databases**. Extract **carillon.db** to a temporary folder and restore it as **databases\carillon.db** on the USB. Skip this step if the 8.1 database already had a v9 dispatch table and startup made no backup. If SQLite journal or WAL files are still present after closing the app, have the database files checked before replacing them.
3. Start the original executable for version 7.2.4 or 8.1 from the USB. It uses the same music folders.
4. Check the legacy playlist, schedule and output before resuming playback.

# Create a separate v9 installation

Use **CarillonMigration.exe** from the release folder when you want a new folder containing a copy of your older installation. Keep it beside **CarillonBells.exe** from the same architecture folder, either **32-bit** or **64-bit**. The assistant copies that main executable into the new installation.

1. Close the older app and launch **CarillonMigration.exe**. Select its installation folder in **Source**.
2. Enter a new folder path in **Destination**. Check the version, paths, music summary and any warnings in **Review**.
3. Check the box and click **Back Up and Migrate**. Keep **migration-report.txt** and the ZIP in **databases\backup**.
4. Click **Open v9 for review**. Check the copied songs, settings and playback.

# Import only a database into an existing v9 installation

Use **Database import** to replace a v9 database with a copy. Put the music in the folders named in the imported playlist, such as **Songs\Daily** beside the executable.

1. Open **Database import** in the v9 sidebar. Verify the local v9 database path.
2. Click **Analyze source database** and choose the legacy **.db**. Check for **SQLite integrity: ok**. Scroll through the schema to inspect tables, views, indexes and triggers. Click **Save report** if you want to keep the analysis.

![The analysis shows the SQLite integrity result and directory format, followed by the scrollable schema.](figures/analysis.png)

3. Click **Choose and import a database copy** and select the database you analyzed. V9 reads the source, validates the candidate and backs up the current v9 database before replacing it.
4. Check the playlist, random slot and silence counts. The **Local ZIP backup** under **databases\backup** holds the previous v9 database.

![The import report lists the local v9 backup ZIP and the imported playlist and directory counts.](figures/import.png)
