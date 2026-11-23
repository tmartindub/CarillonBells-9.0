# Start Carillon

1. In File Explorer, right-click the English 64-bit deployment ZIP and choose **Extract All**. Extract to a writable folder on your PC or USB drive. Keep `CarillonBells.exe` and the supplied `Music` folder together.
2. Open `CarillonBells.exe` from that folder. For a new installation, choose **No** when **Carillon first start** asks whether to upgrade an existing USB drive. Carillon creates `databases\carillon.db` beside the executable. If that database is already present, Carillon opens it directly.
3. Check the PC date and time and select the intended speakers or amplifier as the Windows sound output. Clear **Schedule enabled** while you set up and test. Carillon enables the schedule when you start it normally.

**Upgrading from 7.2.4 or 8.1?** Open the v9 executable from a fresh extracted folder on your PC and choose **Yes** at the first-start prompt. Follow the **Database Migration Guide** to back up and upgrade your existing USB drive. The assistant uses the database and music already on the drive.

Start Carillon from the USB to use its database and music, even if Windows assigns a different drive letter. Close Carillon before unplugging the USB or copying its installation folder to another drive.

# Add music and test playback

1. Use a supplied MP3 or copy your own MP3 or WAV into the **Music** folder beside the executable.
2. Click **Add files** below **Play schedule**, open the **Music** folder, and select a recording. It appears in **Songs**. On a prepared installation, you can also select a recording that is already listed.
3. Set Windows volume low for the test. If the player shows **Unmute**, click it. Use **System volume** to adjust the level, then select the recording and click **Play**. Listen for sound, then click **Stop**.

![The demo shows Songs in the center, Add files on the right, and the player below. Keep Schedule enabled cleared during setup.](figures/main.png)

**Set the displayed name.** In the sidebar, open **System**, enter an **Organization name**, then click **Save** and **Close**. Leave **Playback logging** checked so you can confirm the test in **Log**.

# Set and verify a play time

1. Select your recording. Under **Play on these days**, check each day on which it may play automatically. Clear the checkboxes for the days it should skip.
2. Set **From** and **Until** so the range includes today. Both dates are included. Set **Group** to **Daily List** to use the song’s own times, and set **Times to play** to `1`.
3. Under **Play times**, enter a time a few minutes ahead of the current PC time and leave the other boxes blank. For example, `18:00:00` means 6:00 p.m. Click **Save**.

![The demo shows one play at 18:00:00 with all seven days checked. Use Daily List for your own test.](figures/schedule.png)

## Confirm the first scheduled play

Check **Schedule enabled**. Open **Today's schedule** from the sidebar and look for the recording under **Remaining queue**. The **Next Song** card shows the first item due. Keep Carillon open and listen at the scheduled time, then check **Log** for the **Played Song** entry.

If the recording is missing from the queue, check today’s weekday box, the dates, the saved time, and **Silence windows**. If the player runs without sound, check the Windows output, mute, and volume. Use **Help** in the sidebar or the **User Guide** for daily operation.
