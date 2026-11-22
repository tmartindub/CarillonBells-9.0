# Carillon Bells 9.0

Experimental C# / WPF edition of Westminster Chimes and Carillon Bells. This is a separate application and repository. The working Delphi 8.1 source, GitHub repository, website and production downloads remain unchanged.

The application uses the approved WPF composition, portable SQLite storage, independent managed MP3 decoding, PCM WAV playback, Windows endpoint volume/mute, scheduling, seasonal groups, annual random-directory rotation, both weekly and legacy silence systems, SMTP health reports, logs, themes, original analog clock artwork and local HTML help.

## Running

Extract the complete architecture-specific self-contained evaluation ZIP into a new writable folder and run `Launch-Carillon-Evaluation.cmd`. The included .NET runtime does not require a machine-wide installation. The deployment contains multiple runtime/native files; it is not a statically linked single EXE.

The first start can create an empty database. Database import copies a selected database read-only into the separate installation, backs up existing local data as ZIP, and upgrades only the copy. Music is not included or copied automatically. Do not run this application against production storage.

`--evaluation` suppresses automatic email. It retains real scheduling, playback, power hold, and Windows volume control. Manual email test commands ask for confirmation.

## Building and testing

Use a supported .NET 10 SDK and Windows desktop development tools. All windows and controls are editable XAML. Open `CarillonBells.sln` in an IDE supporting .NET 10 / WPF.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Debug -Runtime win-x86
powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -Configuration Release -Runtime win-x64 -Publish
```

The SDK used for development is installed locally under `tools\dotnet`, and caches are ignored. A fresh checkout can use an installed .NET 10 SDK. Build intermediates go to `dcu`.

```powershell
.\CarillonBells.exe --self-test
.\CarillonBells.exe --audio-test "C:\path\to\reference.mp3"
```

Self-tests use disposable sanitized data. The audio test reads the supplied MP3 without modifying it, plays a quiet generated WAV, and exercises scheduled playback while the clock is open. Results are written beside the executable. Do not send real email as part of automated validation.

## Documentation

- `docs\guides\CarillonBells_9_0_User_Guide.docx`
- `docs\guides\CarillonBells_9_0_Engineering_Guide.docx`
- Companion PDFs under `docs\pdf`
- `help\index.html` for offline help, search, glossary, index and print support; GTranslate requires internet access
- `docs\guides\acceptance-matrix.md` for implemented behavior, executed tests and outstanding environmental acceptance
- `docs\guides\source-inventory.json` and `database-schema.json` for reference evidence without configuration values

The guides have title pages, last-changed dates and changeable TOC fields. In LibreOffice use Tools → Update → Update All after editing.

## Behavioral decisions

Missed events after sleep or clock jumps are skipped. A persisted dispatch ledger prevents replay of an already dispatched local-date occurrence. Full-day reports do not consume the live queue. The source distinction between inclusive compile-time silence ends and exclusive live silence ends is preserved. Playlist edits and administration restore the previous schedule state when finished; an already paused schedule stays paused. The playlist reloads automatically after changes and import, preserving the selected song by its stable key, current search, and group expansion. There is no manual playlist Refresh button.

## Release status

Source backup publication does not authorize replacing the live 8.1 release. Multiple actual DPI settings, storage disconnection, device removal, authorized SMTP delivery and extended soak testing remain deployment acceptance checks. See the acceptance matrix. Do not label this evaluation as production-approved.

New code is Apache License 2.0. `NOTICE` retains the MIT notice for reused reference material. `third-party-notices` contains dependency notices. Private databases, credentials, logs, dumps and music must never enter public Git or packages.
