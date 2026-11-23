# Start the app

On the PC that will run the app, open **Settings**, then **System**, then **About**. Under **System type**, use **win-x86** for 32-bit Windows or **win-x64** for 64-bit Windows on an x64 PC.

1. Choose the matching **USB New Install** ZIP. If you have **Package Setup**, run it on the PC to save that ZIP first. In File Explorer, right-click the ZIP and choose **Extract All**. Extract it to a new USB drive or writable PC folder, keeping `CarillonBells.exe` beside `databases` and `Music`.
2. Check the PC date and time and select the intended Windows speakers or amplifier. Set a low volume if you are testing.
3. Open `CarillonBells.exe` where you extracted it. The included database opens with basic time-marking chimes already scheduled. Clear **Schedule enabled** while you set up and test.

**Upgrading from 7.2.4 or 8.1?** Close the old program. Use the matching **USB Upgrade** ZIP on the existing USB. Follow the **Database Migration Guide** in **Documentation**, then run the new executable from the USB to use your database and music.

Start the app from the USB to use its database and music, even if Windows assigns a different drive letter. Close the app before unplugging the USB or copying its installation folder to another drive.

# Add music and test playback

1. Select a supplied chime in **Songs**, or copy your own MP3 or WAV into the **Music** folder beside the executable.
2. To add your own recording, click **Add files** below **Play schedule**, open the **Music** folder, and select the recording. It appears in **Songs**.
3. Set Windows volume low for the test. If the player shows **Unmute**, click it. Use **System volume** to adjust the level, then select the recording and click **Play**. Listen for sound, then click **Stop**.

![The demo shows Songs in the center, Add files on the right, and the player below. Keep Schedule enabled cleared during setup.](figures/main.png)

**Set the displayed name.** In the sidebar, open **System**, enter an **Organization name**, then click **Save** and **Close**. Leave **Playback logging** checked so you can confirm the test in **Log**.

# Set and verify a play time

1. Select your recording. Under **Play on these days**, check each day on which it may play automatically. Clear the checkboxes for the days it should skip.
2. Set **From** and **Until** so the range includes today. Both dates are included. Set **Group** to **Daily List** to use the song’s own times, and set **Times to play** to `1`.
3. Under **Play times**, enter a time a few minutes ahead of the current PC time and leave the other boxes blank. For example, `18:00:00` means 6:00 p.m. Click **Save**.

![The demo shows one play at 18:00:00 with all seven days checked. Use Daily List for your own test.](figures/schedule.png)

## Confirm the first scheduled play

Check **Schedule enabled**. Open **Today's schedule** from the sidebar and look for the recording under **Remaining queue**. The **Next Song** card shows the first item due. Keep the app open and listen at the scheduled time, then check **Log** for the **Played Song** entry.

If the recording is missing from the queue, check today’s weekday box, the dates, the saved time, and **Silence windows**. If the player runs without sound, check the Windows output, mute, and volume. Use **Help** in the sidebar or the **User Guide** for daily operation.
