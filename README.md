# Westminster Chimes and Carillon Bells 9.0

Experimental C# / WPF edition of Westminster Chimes and Carillon Bells. This is a separate application and repository. The working Delphi 8.1 source, GitHub repository, website and production downloads remain unchanged.

The application uses the approved WPF composition, portable SQLite storage, independent managed MP3 decoding, PCM WAV playback, Windows endpoint volume/mute, scheduling, seasonal groups, annual random-directory rotation, both weekly and legacy silence systems, SMTP health reports, logs, themes, original analog clock artwork and local HTML help.

## Running from USB

Read the **Database Migration Guide** before upgrading an existing installation. Make a complete pre-upgrade ZIP backup off the USB, including the old program, database and recordings. **New Install** supplies a starter database for an empty USB; never use it to preserve an existing schedule. Public downloads are Installer ZIPs: extract on the computer, then run the enclosed Setup. Extracting an Installer ZIP alone does not install the bell program.

**Method A upgrades the existing installation.** Install the Upgrade program files beside the existing databases folder, then double-click `CarillonBells.exe` there. For 7.2.4, the main program itself asks permission, saves `databases/carillon_before_upgrade_*.zip`, converts the working database and opens a paused review session. It does not launch CarillonMigration.exe. For 8.1 it opens the existing database and backs up before required compatibility additions. A normal 8.1 startup enables scheduling, so use a safe test output. Music stays in place.

**Method B creates a separate copy.** After installing the matching Upgrade program files, double-click `CarillonMigration.exe` instead of starting CarillonBells.exe against the original database. Select the old source, an existing parent such as `E:\`, and an unused folder name such as `V9`. The assistant creates `E:\V9`; do not create it yourself. It copies the database and available supported recordings and supplies v9 programs/guides, leaving the source database and recordings unchanged. It is not a whole-drive clone. Edits in the two databases do not synchronize. Starting Method A first means the source is already upgraded; making a copy afterward does not undo that upgrade.

**Returning to the old version depends on the method.** Close v9 and preserve its later state in another complete ZIP first. After Method B, resume the unchanged old program from its original folder if that source was never upgraded. After Method A from 7.2.4, use CarillonMigration.exe → Restore 7.2.4 with the verified original database ZIP. After Method A from 8.1, recover the complete original 8.1 backup or follow the guide's database-only procedure. Restoring an old database does not merge later v9 edits. Keeping the old executable alone does not restore the old data. The 7.2.4 restore button is not for 8.1 backups.

Choose the release for the installed Windows system type under **Settings > System > About**. Use `win-x86` for 32-bit Windows and `win-x64` for 64-bit Windows on an x64 processor. Both English builds include the .NET 10 runtime and follow the supported Windows editions in the [Microsoft .NET compatibility table](https://learn.microsoft.com/en-us/dotnet/core/install/windows#supported-versions).

Publish the four **Installer ZIPs** from `export\downloads\32-bit` and `export\downloads\64-bit`. Each contains a self-contained Setup EXE, its PDF guide and `START_HERE.txt`. Right-click the download, choose **Extract All** onto the computer, then double-click the extracted Setup EXE and select the USB installation folder. Click **Install**, then **Verify** before starting Carillon. No separate portable ZIP is needed by the user. Upgrade Setup preserves the existing database and music.

The `export\32-bit` and `export\64-bit` folders hold build inputs and advanced manual-install packages: the app and Migration Assistant EXEs and the portable New Install/Upgrade ZIPs containing loose files. Those portable ZIPs are required by `tools\build_usb_setups.py`; keep them. They are different from the public Installer ZIPs ending `_Package_Setup.zip`. Redundant loose Setup EXEs are removed after their Installer ZIPs pass validation. Windows security warnings can still appear after ZIP extraction; ZIP packaging does not confer a trusted publisher signature or SmartScreen reputation.

To move a working installation to another writable USB, close Carillon and copy the complete working installation. Choose the executable that matches the Windows system where you will run it.

For a separate database copy, **Database import** opens the source read-only, backs up the local database, and applies compatibility changes to the imported copy. See the Database Migration Guide in `Documentation` for both upgrade routes and recovery.

Scheduled and manual playback, including random music, resolve inside the running EXE's folder. Legacy drive-letter paths use their folder layout under that folder (for example, `C:\Songs\bell.mp3` resolves to `<EXE folder>\Songs\bell.mp3`). Store music and random directories as relative paths. Copy new audio into the portable installation before adding it; network paths and paths escaping the installation are rejected. Database import does not copy audio. Existing music folders are preserved during binary updates.

The original interface uses an 18-unit base font, with larger headings and 14–16-unit supporting labels. System → Font settings offers seven family choices and base sizes 14–20; Check every page validates current data and minimum-window layouts before Apply checked font can save a choice. Apply shows a clear applied-and-saved confirmation. Custom preferences in data\fonts.json are revalidated at startup/display-scale changes. The main workspace fits the window and reserves bottom rows for the player, volume and status, so those controls stay visible. The song list scrolls within its panel; very small windows can scroll navigation or editor content independently. System settings use three columns, and Email settings use two columns. Group play times display as HH:mm:ss without fractional seconds. These settings and layouts remain editable in XAML.

Playback logs record `Played Song:` entries. Existing `Songs`, `Random_songs`, and other personal music folders can still be used on upgraded USB drives when the database references them.

`--evaluation` suppresses automatic email. It retains real scheduling, playback, power hold, and Windows volume control. Manual email test commands ask for confirmation.

Saved play times use 24-hour HH:mm:ss. Users can enter AM/PM times: 6:15 PM saves and displays as 18:15:00. The runtime requests that Windows stay awake and writes data\keepalive.txt periodically to keep the installation drive active; the computer must remain powered and the drive connected.

## Building and testing

Use a supported .NET 10 SDK and Windows desktop development tools. All windows and controls are editable XAML. Open `CarillonBells.sln` in an IDE supporting .NET 10 / WPF.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Release -Runtime win-x86 -Publish -SingleExe
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Release -Runtime win-x86 -Publish -MigrationSingleExe
powershell -NoProfile -ExecutionPolicy Bypass -File tools\package.ps1 -Runtime win-x86 -SingleExe -UsbUpgrade
powershell -NoProfile -ExecutionPolicy Bypass -File tools\package.ps1 -Runtime win-x86 -SingleExe -StarterDatabase <path-to-starter-db> -MusicDirectory <path-to-Music>
```

Repeat the architecture-specific steps with `win-x64` to build the 64-bit release.

The SDK used for development is installed locally under `tools\dotnet`, and caches are ignored. A fresh checkout can use an installed .NET 10 SDK. Build intermediates go to `dcu`.

```powershell
.\CarillonBells.exe --self-test
.\CarillonBells.exe --audio-test "C:\path\to\reference.mp3"
```

Self-tests use disposable sanitized data. The audio test reads the supplied MP3 without modifying it, plays a quiet generated WAV, and exercises scheduled playback while the clock is open. Results are written beside the executable. Do not send real email as part of automated validation.

## Documentation

- `docs\guides\CarillonBells_9_0_User_Guide.docx`
- `docs\guides\CarillonBells_9_0_Engineering_Guide.docx`
- `docs\guides\CarillonBells_9_0_Database_Migration_Guide.docx`
- `docs\guides\CarillonBells_9_0_Quick_Start_Guide.docx`
- Companion PDFs under `docs\pdf`
- `help\index.html` for offline help, search, glossary, index and print support; GTranslate requires internet access
- `docs\guides\acceptance-matrix.md` for implemented behavior, executed tests and outstanding environmental acceptance
- `docs\guides\source-inventory.json` and `database-schema.json` for reference evidence without configuration values

The guides have title pages, last-changed dates and changeable TOC fields. In LibreOffice use Tools → Update → Update All after editing.

## Behavioral decisions

Missed events after sleep or clock jumps are skipped. A persisted dispatch ledger prevents replay of an already dispatched local-date occurrence. Full-day reports do not consume the live queue. The source distinction between inclusive compile-time silence ends and exclusive live silence ends is preserved. Playlist edits and administration restore the previous schedule state when finished; an already paused schedule stays paused. The playlist reloads automatically after changes and import, preserving the selected song by its stable key, current search, and group expansion. There is no manual playlist Refresh button.

## Current UI and editing behavior

- Calendar buttons appear on date-field hover or keyboard focus and remain visible while the popup is open. Reserved space prevents text movement.
- Groups prompts only for actual value changes, additions or deletions; opening/closing and reverting all edits do not prompt.
- Random music uses annual month/day input such as 10/1, without a year, and supports ranges crossing New Year.
- PLAYER records the actual last-started recording independently of selection, with wrapping clear of System Volume.
- Clock hands preserves Original and supplies eighteen slender black alternatives, with the original face and preview geometry.
- The legacy Ash Wednesday end remains stored but hidden and synchronized to the visible date. Startup repairs stale values after a database ZIP backup.
- Search × clears the filter. Playlist refresh is automatic, maintenance logs identify pause reasons, and successful email clears a stale email-failure notice.
- Both final native Windows builds passed 204 checks. Tommy also confirmed that the corrected Groups behavior tests well on D:. This is specific acceptance evidence, not blanket production qualification.

## Release status

Source backup publication does not authorize replacing the live 8.1 release. Multiple actual DPI settings, storage disconnection, device removal, authorized SMTP delivery and extended soak testing remain deployment acceptance checks. See the acceptance matrix. Do not label this evaluation as production-approved.

New code is Apache License 2.0. `NOTICE` retains the MIT notice for reused reference material. `third-party-notices` contains dependency notices. Keep working databases, credentials, logs, dumps, and personal music out of public Git and release packages. The New Install ZIP uses a sanitized starter database and the supplied `Music` collection.
