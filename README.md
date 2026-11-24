# Westminster Chimes and Carillon Bells 9.0

Experimental C# / WPF edition of Westminster Chimes and Carillon Bells. This is a separate application and repository. The working Delphi 8.1 source, GitHub repository, website and production downloads remain unchanged.

The application uses the approved WPF composition, portable SQLite storage, independent managed MP3 decoding, PCM WAV playback, Windows endpoint volume/mute, scheduling, seasonal groups, annual random-directory rotation, both weekly and legacy silence systems, SMTP health reports, logs, themes, original analog clock artwork and local HTML help.

## Running from USB

- **New installation.** Extract the USB New Install ZIP to an empty USB root. Open `CarillonBells.exe`. The ZIP supplies a starter time-marking schedule in `databases\carillon.db` and the `data`, `Documentation`, `Help`, `logs`, and `Music` folders.
- **Upgrade from 7.2.4.** Close the old app, extract the USB Upgrade ZIP into the existing USB root, and open `CarillonBells.exe`. Confirm the one-step conversion. V9 verifies a backup ZIP in `databases`, converts the database in place, and opens with scheduling paused for review.
- **Upgrade from 8.1.** Close the old app, extract the USB Upgrade ZIP into the existing USB root, and open `CarillonBells.exe`. V9 uses the existing database. On the first run that needs the v9 dispatch table, it saves a backup ZIP in `databases` before adding the table.

The upgrade ZIP contains `CarillonBells.exe`, the matching `CarillonMigration.exe`, and updated files for `Documentation`. Both upgrades use the existing USB folders and music. Open `CarillonBells.exe` directly from the USB.

**Restore 7.2.4.** Close `Carillon.exe` and `CarillonBells.exe`. From `CarillonMigration.exe`, choose **Restore 7.2.4…**, select the USB and its pre-upgrade ZIP, then review the restore. The assistant backs up the current database and restores the verified original. Start the original `Carillon.exe` from the USB when ready to check the schedule.

Choose the release for the installed Windows system type under **Settings > System > About**. Use `win-x86` for 32-bit Windows and `win-x64` for 64-bit Windows on an x64 processor. Both English builds include the .NET 10 runtime and follow the supported Windows editions in the [Microsoft .NET compatibility table](https://learn.microsoft.com/en-us/dotnet/core/install/windows#supported-versions).

The `export\32-bit` and `export\64-bit` release folders each contain `CarillonBells.exe`, `CarillonMigration.exe`, the New Install and Upgrade USB ZIPs, and their Package Studio Pro setup executables. A Package Studio setup stages its matching ZIP in a PC folder. Extract that ZIP to the USB root. For the separate-folder Migration Assistant, keep the matching `CarillonBells.exe` beside `CarillonMigration.exe`.

To move a working installation to another writable USB, close Carillon and copy the complete working installation. Choose the executable that matches the Windows system where you will run it.

For a separate database copy, **Database import** opens the source read-only, backs up the local database, and applies compatibility changes to the imported copy. See the Database Migration Guide in `Documentation` for both upgrade routes and recovery.

Scheduled and manual playback, including random music, resolve inside the running EXE's folder. Legacy drive-letter paths use their folder layout under that folder (for example, `C:\Songs\bell.mp3` resolves to `<EXE folder>\Songs\bell.mp3`). Store music and random directories as relative paths. Copy new audio into the portable installation before adding it; network paths and paths escaping the installation are rejected. Database import does not copy audio. Existing music folders are preserved during binary updates.

The interface uses an 18-pixel base font, with larger headings and 14–16-pixel supporting labels. The main workspace fits the window and reserves bottom rows for the player, volume and status, so those controls stay visible. The song list scrolls within its panel; very small windows can scroll navigation or editor content independently. System settings use three columns, and Email settings use two columns. Group play times display as HH:mm:ss without fractional seconds. These settings and layouts remain editable in XAML.

Playback logs record `Played Song:` entries. Existing `Songs`, `Random_songs`, and other personal music folders can still be used on upgraded USB drives when the database references them.

`--evaluation` suppresses automatic email. It retains real scheduling, playback, power hold, and Windows volume control. Manual email test commands ask for confirmation.

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

## Release status

Source backup publication does not authorize replacing the live 8.1 release. Multiple actual DPI settings, storage disconnection, device removal, authorized SMTP delivery and extended soak testing remain deployment acceptance checks. See the acceptance matrix. Do not label this evaluation as production-approved.

New code is Apache License 2.0. `NOTICE` retains the MIT notice for reused reference material. `third-party-notices` contains dependency notices. Keep working databases, credentials, logs, dumps, and personal music out of public Git and release packages. The New Install ZIP uses a sanitized starter database and the supplied `Music` collection.
