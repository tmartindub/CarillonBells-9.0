# Before you start

This guide is for a new installation of Westminster Chimes and Carillon Bells 9.0. It takes you from the download to one song playing on schedule. If your USB already contains a working version 7.2.4 or 8.1, use the **Database Migration Guide** instead; it explains how to keep your existing songs and settings.

Choose **New Install** for the computer that will run the bells. In Windows, open **Start → Settings → System → About** and find **System type**. If the line begins **64-bit operating system** and says **x64-based processor**, choose the **64-bit** download. If it begins **32-bit operating system**, choose **32-bit**. The filename uses **win-x64** for 64-bit or **win-x86** for 32-bit. The Migration Guide explains these choices in more detail.

You may receive a ZIP file or a **Package Setup** program. A ZIP is a compressed collection of files. Package Setup saves that ZIP to a folder on your computer; you still need to unpack it onto the USB as described below.

# Put the new installation on the USB

1. Plug an empty USB drive into the computer. Open **File Explorer**, choose **This PC**, and identify that drive. Its letter may be D:, E:, or something else. Use the letter shown on your computer.
2. If you downloaded **Package Setup**, open it and follow its screens to save the New Install ZIP. Note the folder it uses. If you downloaded the ZIP directly, it will normally be in **Downloads**.
3. Right-click the New Install ZIP and choose **Extract All**. Select the empty USB drive as the destination, then click **Extract**. Wait until Windows finishes copying the files.
4. Open the USB in File Explorer. You should see **CarillonBells.exe** alongside the **databases**, **Music**, and **Documentation** folders. If Windows hides file extensions, the program appears as **CarillonBells**. Open this program from the USB, rather than from inside the ZIP or the download folder.

The download includes the software needed to run the app. You do not need to install .NET separately. Keep the supplied folders together: the database holds the schedule, while Music holds the recordings.

<!-- page break -->

# Open the app and test a song

1. Check that Windows shows the correct date and time. Select the speakers or amplifier you intend to use and set a low test volume.
2. Open **CarillonBells** from the USB. When the main screen appears, clear **Schedule enabled** at the upper right while you set up. The starter installation already has chimes scheduled.
3. Open **System** in the left panel. Enter your **Organization name**, click **Save**, then **Close**. Leave **Playback logging** checked so you can review your test in Log.
4. In **Songs**, select a supplied chime. Click **Play** at the bottom, or double-click the song. If the button says **Unmute**, click it first. Listen, then click **Stop**. The player displays **Last song played**; selecting another song does not change that record.

To add your own recording later, copy its MP3 or WAV file into Music on this USB, then use **Add files** in the app. Copying a file alone does not add it to the Songs list.

# Set one play time

1. Select the song you want to test. In **Play schedule** on the right, check the days on which it may play.
2. Set **From** and **Until** so today falls between them. Dates use U.S. month/day/year order. Point at a date box to reveal its calendar button, click the button, then choose the day. The button also appears when you reach the field with the keyboard.
3. Set **Group** to **Daily List** so the song uses its own play times. Set **Times to play** to **1**.
4. Enter a time a few minutes ahead of the current time in the first **Play times** box. Leave the other boxes blank. Click **Save**.

**Time entry:** the app displays saved play times in **24-hour format**. You can type **6:15 PM** or **18:15**; after saving, both display as **18:15:00**. Likewise, **8:15 AM** becomes **08:15:00**. **12:00 AM** means midnight (**00:00:00**), and **12:00 PM** means noon (**12:00:00**). A blank box means no play at that position.

![This example has one play time, 18:00:00, which means 6:00 PM. Use a time a few minutes ahead for your own test.](figures/schedule.png)

# Confirm the scheduled play

1. Check **Schedule enabled**. Open **Today's schedule** in the left panel and find your song under **Remaining queue**. Use **Back to playlist** to return.
2. Check the **Next Song** card. Leave the app open and listen when the scheduled time arrives.
3. Open **Log** and look for a **Played Song** entry. If nothing played, check the saved time, today's weekday, the date range, and **Silence windows**. If the player moved but you heard nothing, check Windows audio output, mute, and volume.

For everyday work, use the **User Guide** or the app's **Help** button. To change text size, open **System → Font settings**, choose a font and size, and click **Check every page**. Use **Apply checked font** only after the check passes; the confirmation says the choice is already applied and saved.

For your overnight test, leave the app open, scheduling enabled, the USB connected, and the computer powered on. The app requests that Windows stay awake and writes to the USB periodically to keep it active. Check the next morning's playback and report. Close the app before unplugging the drive.
