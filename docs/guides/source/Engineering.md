# Release scope and design

## Purpose and migration from 8.1

Carillon Bells 9.0 is the C# Windows Presentation Foundation edition of Westminster Chimes and Carillon Bells. It replaces the Delphi FireMonkey implementation in the new v9 installation while retaining the grouped playlist, local-time schedules, seasonal dates, rotating random music, weekly and legacy silence, mail reports, and the original analog clock face. This edition documents application source revision 3eb90c0.

The new implementation is a separate application with its own executable, database, and Git history. A database copied from 8.1 supplies the starting playlist and settings. The v9 import flow reads the source database, upgrades a candidate copy, and backs up the local v9 database before replacement. The repository preserves compatible tables and custom schema objects. The standalone 8.1 conversion utility is historical. The Database tab now supplies the analysis, import, and conversion report.

## Source map

- App.xaml and App.xaml.cs define WPF resources, startup mode, database setup, and exclusive file handles.
- MainWindow.xaml and AdministrationWindow.xaml define the editable presentation. Their code-behind binds the state, routes actions, and resolves pending edits.
- CarillonRuntime.cs coordinates scheduling, dispatch, silence, logging, and scheduled maintenance. Domain.cs holds song data, date and time normalization, path handling, and the schedule and calendar rules.
- CarillonRepository.cs owns SQLite statements, transactions, compatibility upgrades, and backups. AudioPlayback.cs contains the playback and Windows endpoint paths.
- EmailReports.cs and CarillonLog.cs supply reports and diagnostic evidence. ClockWindow, ClockHands.xaml, ThemeManager.cs, and Themes.xaml govern appearance and clock rendering.
- RegressionTests.cs, AudioRegressionTests.cs, and the build and package scripts supply automated verification and deployment.

Use the versioned source to resolve behavioral questions. The FMX handoff and original 8.1 publications are useful historical references. The current acceptance matrix records which verification was actually performed and which checks still depend on the deployment environment.

# Startup and process lifetime

## Opening an installation

Application startup sets the short-date format to en-US MM/dd/yyyy. It handles --self-test and --audio-test before normal initialization, so those modes use dedicated test paths. Normal startup obtains exclusive handles for Carillon.lock beside the executable and carillon.db.lock beside the database. A lock file can remain on disk after an earlier session. Check the process holding the file handle when diagnosing a startup lock.

For a first launch, the app creates the databases folder and asks before initializing an absent carillon.db from the embedded sanitized schema. It then checks compatibility, loads the palette, displays the splash, initializes the runtime, selects the locally saved clock style, and opens the main window. The splash uses the Westminster artwork, title, and portable edition caption. The approved three-bell icon is embedded in the executable and used by the app windows.

The app uses AppContext.BaseDirectory as the root for the database, logs, playback, and help, including when launched from a shortcut with a different working directory. Separate copies on a disk and a USB drive can therefore keep independent data. Run a single scheduling instance on a Windows audio endpoint in normal operation. Two copies with independent queues can both dispatch on the same device.

## Shutdown and power management

The main close path resolves pending playlist edits, closes the separately opened clock, detaches notifications, and disposes the runtime. The runtime stops its dispatcher timer, cancels background mail, releases playback and endpoint subscriptions, and drops the Windows sleep-prevention request. App.OnExit disposes the two lock handles. Exit from the sidebar, File menu, or window close button goes through the same path.

SetThreadExecutionState keeps the system awake during runtime and releases that request when the app exits cleanly. The runtime writes data\keepalive.txt on its 30-second cadence. For a stale timestamp, check the runtime log and process state. Verify amplifier output separately when testing onsite audio.

# Playlist and administration

## Playlist state and editing

MainWindow shows the next-song card, today's counts, active silence, grouped Songs list, Play schedule editor, and player. The title bar reports calendar-unit uptime from the application's last start. The next-song card puts the time above its wrapping title and shows the repeat count when an event repeats. The footer rebuilds the recorded count from the play log and normally displays Songs played today. A runtime error can occupy that status line.

A selected song is identified by the composite playlist_name and song_name key. The editor holds a cloned Song and uses a fingerprint to identify pending changes. Schedule input receives focus at the start of an edit session, which pauses an enabled queue. Save validates the editor, persists the row, and reloads the collection. Cancel restores the selected row's saved values. The earlier schedule state is restored after an edit, so a queue that was already paused remains paused.

Collection reloads preserve the selected key, search, group expansion, and scroll offset where possible. The effective key can change after edits to a playlist row. Navigation uses the visible records under the current filter. Add files reads the duration through the normal decoder. Delete removes the row and leaves the audio file in place.

## Administration state

The Administration window uses tab-specific DataTable views for groups, settings, random folders, and weekly silence. Before Save, it commits active grid cell and row edits and validates dates, enabled silence windows, and other required fields. Settings writes are restricted to the changed columns owned by System and Email. The three owned data grids use transactional writes per table. Treat the administration window as staged UI state and verify any reported save failure before continuing.

Opening most administrative pages pauses scheduling. Themes and Clock hands are exceptions when visited without moving to another administrative tab that requires a pause. The clock and log windows can stay open while background work continues. Cancel changes reloads committed data, and the close flow prompts when changes are pending. Theme selection applies to open windows and persists separately. Clock hand selection previews immediately and uses Save to commit the choice.

# Scheduling and maintenance

## Compiling a day

ScheduleBuilder selects records whose inclusive From and Until dates cover the day and whose weekday flag is enabled. A record has twelve available daily times. Empty values are absent, midnight is a valid time, and duplicate time values in one row collapse into one occurrence. Ordering is by time, then ordinal playlist and path. Simultaneous events for different songs remain distinct, and the player services queued events sequentially.

The full-day schedule and remaining queue have different lifetimes. The full-day snapshot can show earlier events and remains available when dispatch is paused. Rebuilding a live queue excludes events before the current time and applies the dispatch ledger. Weekly silence exclusions use an inclusive end during compilation. Legacy liturgical and funeral checks also affect playback through the live silence coordinator.

## Dispatch and clock changes

CarillonRuntime uses a one-second WPF DispatcherTimer with a reentrancy guard. It compares wall-clock movement with monotonic elapsed time. A timer gap longer than ten seconds, an absolute clock discrepancy greater than two seconds, or a date change triggers a queue rebuild when scheduling is enabled. Detected discontinuities skip missed events. Ordinary playback contention leaves queued events available for sequential dispatch.

The event identifier combines the local yyyyMMddHHmmss timestamp with the song's composite key. Carillon9_Dispatched stores the identifier and event date. Each scheduled event is recorded before playback, including one skipped for live silence. The ledger prevents duplicate dispatch on re-enable or restart and treats a repeated local clock time during the autumn clock change as the same occurrence. A recorded dispatch followed by a playback failure requires separate handling.

The title bar computes uptime using completed calendar months from startup, divided into years and months, followed by weeks, days, hours, minutes, and seconds. The display clamps a wall-clock value earlier than startup to zero. Clock artwork has its own window timer, so closing the clock leaves the scheduling timer active.

## Background timing

- At startup, the runtime trims the play log and reconstructs the recorded count for the current day.
- At or after 00:05, daily schedule and count maintenance runs once per running day while scheduling is enabled.
- At or after 00:10, the log is trimmed when the runtime reaches a new day. This is independent of the playback logging setting.
- January maintenance recalculates liturgical dates from January 2 at 00:15 and seasonal group dates from January 3 at 00:02. The running process tracks completion by year. These checks operate in January, independently of which view is open.
- The automatic morning report becomes due at or after 05:00 once per running day. A process started at or after 05:00 marks the startup date as already handled.

# Seasonal playback and silence

## Seasonal synchronization

CalendarRules calculates Gregorian Easter and recognizes fixed and movable holiday groups. Christmas spans December 22 through December 26, Memorial Day uses the last Monday of May, and Thanksgiving uses the fourth Thursday in November. Recalculation updates recognized names and advances past occurrences. Custom group names are preserved.

When synchronization runs, it matches trimmed seasonal names case-insensitively and excludes Daily List. For matching seasonal rows, the group's dates and first play time replace the row's dates and first time, and the other eleven times are cleared. Engineers investigating a seasonal row whose hand-entered slot disappeared should inspect its group first.

## Random directory selection

Slot 1 is the everyday fallback and uses .\random_songs in a blank v9 installation. Slots 2 through 7 are evaluated in order. The first nonempty folder whose annual date range matches wins. Calendar matching uses month and day, handles a range that crosses New Year, and includes both boundaries. Edits use the Browse button in the intended row. The chosen folder must resolve within the portable installation and is stored as a relative path.

For a song title containing Random Music Generator, the player resolves the active folder at playback and chooses a random MP3 from that directory. The scan uses files directly inside the folder. A missing folder or one without an eligible file causes a playback error. Check the override that was active on the date of the failure before changing the fallback.

## Silence coordination

Silence windows holds twelve reusable weekly records. The weekday on a rule that crosses midnight is its starting weekday. Equal start and end times are inactive. Scheduled compilation excludes the exact weekly end time, while live evaluation releases the window at that end instant. A 22:00–06:00 Friday rule therefore continues into Saturday morning.

The separate System setting enables the legacy liturgical and funeral intervals. Ash Wednesday and All Souls run from 02:05 to 23:55 on their configured dates. The Holy Thursday through Holy Saturday interval runs from 02:05 on the configured Thursday to 23:55 on the configured Saturday. Funeral silence needs complete dates and times, with the end strictly later than the start. The runtime combines active reasons, applies muting on startup or schedule edits, and retains the requested manual mute through overlapping rules.

# SQLite data migration and recovery

## Data model

Microsoft.Data.Sqlite opens short-lived connections with pooling disabled. Repository statements use parameters for values and explicit transactions for compatible table writes. User dates display as MM/dd/yyyy, while normalized persisted dates use yyyy-MM-dd and play times use HH:mm:ss. The parser accepts the compatible legacy date and time representations, including OLE Automation values. Durations are intervals and can exceed one day.

- playlist uses the composite playlist_name and song_name key. Its group is stored as season, and its daily times are scheduled_time1 through scheduled_time12.
- seasonalgroups stores named date ranges and a play time. PL_SETTINGS stores one row under settings_id 1, including distinct System and Email field sets and compatible theme colors.
- PL_RAND_DIR_ROTATION stores slots 1 through 7 with rand_music_dir, frm_date, and to_date. PL_RAND_DIR_ROTATION_WIDE_BACKUP is a preserved historical layout.
- silence_schedule stores a rule ID, enabled flag, purpose, times, and weekday flags. Carillon9_Dispatched stores the persistent local-date dispatch ledger.

samples\empty-schema.sql is the application resource for creating a blank install and test databases. The repository's compatibility checks remain authoritative for an imported database with an older schema.

## Analysis and import

Analyze source database opens the selected file read-only, runs SQLite quick_check, and reports whether the rotation table is row-based. The scrollable report includes definitions from sqlite_schema for tables, indexes, views, and triggers. SQLite-generated objects without separate SQL receive an explanatory line. Save report exports text. Schema names and definitions can still be private, so review a report before sharing it.

![The report shows the rotation table definition, including its slot number constraint and directory and date columns.](figures/analysis.png)

Choose and import a database copy first resolves pending admin changes. The source is opened read-only and SQLite's backup API creates a local candidate. Compatibility changes add required columns, establish a unique settings key, preserve extra tables, and fill required standard slots where needed. Wide rotation data is renamed to a preserved backup and converted to the slot layout. The import stops for review when it finds multiple settings rows. Legacy time normalization applies to the candidate.

A consistent ZIP of the current local database precedes replacement. The app validates the candidate's songs, silence, and settings, releases active playback, replaces the local database, and reloads. If replacement or subsequent initialization fails after the local backup is taken, it can restore the prior snapshot. The final conversion report records the snapshot path and normalized values count. Copy the music separately and verify the corresponding paths under the destination executable.

## Recovery and backups

Close the copy being restored and make a ZIP backup before restoring from a known-good file. Confirm the exact destination and its drive letter. Extract the desired carillon.db from the backup ZIP into the selected copy's databases directory. Reopen in Evaluation mode and verify settings, paths, and upcoming events before resuming normal onsite operation. Database backups live under databases\backup and the project's separate pre-change archive can include source, application files, database snapshots, and the clock preference.

# Audio output and clock visuals

## Decoding and playback lifecycle

AudioPlayback opens MP3 through Mp3FileReaderBase with NLayer's frame decompressor. Both duration reading and playback use that decoder. WAV files use WaveFileReader for PCM, IEEE float, or extensible audio. The samples feed NAudio WaveOutEvent through a 16-bit PCM provider. The runtime receives any decoder or playback faults for logging and display.

The playback controller owns one stream and output object. Play stops and disposes the previous one, begins the selected item, and clamps repeats to 1–5. PlaybackStopped is marshalled to the dispatcher. The callback checks output ownership, rewinds for remaining repeats, and releases resources at completion. Stop clears ownership and unsubscribes before stopping, so the ownership check rejects late playback callbacks. Songs played today counts recorded starts, with all repeats grouped under the same start.

SystemAudio observes the default Windows multimedia render endpoint, subscribes to Core Audio notifications, and rebinds after endpoint changes. The slider and Mute operate on the Windows master endpoint. Managed silence maintains the user's requested manual mute and restores it after the final reason ends. Refresh checks the actual endpoint while the app runs. Include an endpoint and a safe test audio path in playback qualification.

## Clock styles and themes

ClockWindow scales the original face with a Viewbox. Its 1680 × 864 canvas has a pivot at 840,430. ClockHands.xaml defines Original and fourteen numbered DrawingBrush styles. The 460-pixel style tiles use pivot 230,230 and are offset by 610,200 on the live face. The minute hand advances six degrees per minute. The hour hand advances thirty degrees per hour plus a half-degree per minute. The gallery previews the same drawings at 10:10. A saved style updates the open live clock through a shared resource.

The portable installation stores the selected ID in data\clock-hands.txt. The valid range is 0–14, with 0 as Original when the file is absent or invalid. Save writes a temporary sibling and replaces the target. Existing numbered preferences select the same slot when a design is revised. Themes.xaml and ThemeManager control palettes, legacy-compatible stored colors, and separate card tints. These properties and the clock geometry are editable in XAML.

# Reporting and log diagnosis

## Mail and health reporting

Email / reports owns the enable flag, SMTP host and STARTTLS port, credentials, sender, and separate recipients for alerts, tests, and the morning report. MailKit connects using explicit STARTTLS, authenticates, sends, and records success after the transport operation returns. Invalid configuration, connection failure, and rejected recipients surface as errors. Manual sends ask for confirmation. Evaluation suppresses automatic delivery, while real playback, endpoint controls, and local reporting remain active.

An HTML health report uses an independent full-day schedule and includes the upcoming events, recorded plays, playlist duration, portable and system drive space, boot and runtime uptime, physical memory, audio output, recent maintenance, and missing-file checks. Preview report saves a local HTML file. Inspect that output first. Test actual SMTP only with authorized recipients and credentials on the intended network.

## Logs and fault isolation

CarillonLog serializes file access and recognizes both current en-US and historical ISO-style timestamps. Played Song entries reconstruct the recorded count. A repeat stays part of one play, and a separate completion message is unnecessary. Trimming removes old valid dates after the thirty-day cutoff while retaining malformed historical entries. The Log window reads latest entries and can refresh while playback remains active.

- For an unavailable track, compare the resolved filename with the executable's folder, then verify the active seasonal slot and the relative path.
- For a missing scheduled event, inspect dates, weekdays, times, current queue, silence, time discontinuities, and the dispatch ledger before creating a new event.
- For silent playback, inspect the actual endpoint, endpoint volume and mute, current silence reasons, and decoder or path errors.
- For startup failure, verify the intended installation's writability, runtime architecture, exclusive handle ownership, and SQLite integrity. Use the archive for recovery after confirming which copy is affected.

# Build and release

## Project and build commands

Open CarillonBells.sln in a Windows IDE with .NET 10 and WPF support. CarillonBells.csproj targets net10.0-windows. Directory.Build.props routes project intermediates into dcu and excludes local output, tools, and private directories from compilation. The executable icon, splash and clock assets are WPF resources. Help is local HTML copied into published output.

Run tools\build.ps1 with Release or Debug. To publish a portable build, supply -Runtime win-x64 or -Runtime win-x86 and -Publish. The script uses tools\dotnet when installed, otherwise it uses the system dotnet executable, and directs CLI settings and NuGet packages into project-local caches. The lock file and project file identify dependency versions. Self-contained packages include the architecture-specific .NET Desktop and SQLite components and require the complete folder.

Use an approved pre-change ZIP before updating a working installation. Close Carillon and validate the application build first. The optional -PortableRoot argument applies only to an initialized v9 destination with its own database and executable. It copies and hash-verifies application files while preserving the portable database, audio, logs, and data directory. Recheck the drive letter and start from the intended EXE. To relocate, copy the complete folder including the database, relative music tree, data, and help.

## Validation and packaging

Run CarillonBells.exe --self-test on disposable data before deployment and on each updated installation. Verify the generated regression-results.txt and failure count alongside the process exit code. Run --audio-test followed by a local reference MP3 to test decoding, quiet WAV playback, serial repeats, pause and stop, dispatch with Clock open, time discontinuity, silence, endpoint mute, and cleanup. Audio tests exercise actual Windows audio, so run them in a safe test setting. Review audio-regression-results.txt. Exercise exact layouts, deployed resources, save and cancel, database import and restoration, and portability under a different working directory when those surfaces change.

tools\package.ps1 creates architecture-specific Evaluation ZIPs and records file lengths, SHA-256 hashes, and the source commit in the package manifest. Audit the ZIP for private files and verify its manifest. Carry the license, notices, local help, and current guides into the package. Keep working databases, credentials, music, diagnostic logs, backups, and scratch output private. Record physical endpoint removal, USB disconnect behavior, real SMTP delivery, diverse monitor scaling, and long-running operation as separate deployment evidence when checked.

## Maintain the guides

The editable manuscripts are under docs\guides\source. Run tools\build_guides.py with an output folder to generate the DOCX drafts. In LibreOffice Writer, update each table of contents, save the DOCX, and export the PDF from that document. Check the dotted leaders, page numbers, and links after updating the contents. The release package collects the finished DOCX guides from docs\guides and their PDFs from docs\pdf.
