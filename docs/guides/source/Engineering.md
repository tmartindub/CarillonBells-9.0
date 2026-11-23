# Release scope and design

## Purpose and upgrade behavior

Version 9.0 of Westminster Chimes and Carillon Bells uses C# and Windows Presentation Foundation. It replaces the Delphi FireMonkey implementation while retaining the grouped playlist, local-time schedules, seasonal dates, rotating random music, weekly and legacy silence, mail reports, and the original analog clock face.

The single-executable release supports a new USB with a starter database and in-place upgrades of existing 7.2.4 and 8.1 installations. An in-place upgrade runs from the existing folder and uses its database and audio tree. The Database tab separately supports read-only analysis and import of a selected database into an existing v9 installation.

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

The new USB package supplies databases\carillon.db and its time-marking recordings. The single-executable launch reads the database in its own installation folder. A 7.2.4 database triggers one confirmation before conversion and opens a review session with scheduling paused and automatic mail suppressed. A compatible 8.1 database opens directly with the normal runtime; the application backs up the file before initializing any missing dispatch metadata.

After the database is ready, startup loads the palette, displays the splash, initializes the runtime, selects the locally saved clock style, and opens the main window. The splash uses the Westminster artwork, title, and portable edition caption. The approved three-bell icon is embedded in the executable and used by the app windows.

The app uses AppContext.BaseDirectory as the root for the database, logs, and playback, including when launched from a shortcut with a different working directory. The single executable opens its embedded HTML Help from a temporary PC copy. Separate copies on a disk and a USB drive keep independent data. Run a single scheduling instance on a Windows audio endpoint in normal operation. Two copies with independent queues can both dispatch on the same device.

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

Slot 1 is the everyday fallback. The empty test schema gives it .\random_songs. Slots 2 through 7 are evaluated in order. The first nonempty folder whose annual date range matches wins. Calendar matching uses month and day, handles a range that crosses New Year, and includes both boundaries. Edits use the Browse button in the intended row. The chosen folder must resolve within the portable installation and is stored as a relative path.

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

The empty schema resource at samples\empty-schema.sql supplies a baseline for disposable databases. The release starter is packaged as databases\carillon.db with basic time-marking entries whose relative paths resolve under Music. The repository's compatibility checks also apply to a separately imported database.

## In-place upgrade

For 7.2.4, startup detects the wide random-directory schema and requests confirmation before converting it. It saves a verified carillon_before_upgrade_*.zip beside the working database. The converted schema retains the original wide table and fills missing compatibility fields and silence slots. It preserves the stored schedule times and the existing audio. A normal start of a compatible 8.1 database adds Carillon9_Dispatched when needed and places the pre-change ZIP beside the database.

The in-place ZIP contains carillon.db. For release verification, use disposable legacy installations and confirm the preserved snapshot, readable working database, and unchanged existing music before resuming on-site playback.

## Analysis and import

Analyze source database opens the selected file read-only, runs SQLite quick_check, and reports whether the rotation table is row-based. The scrollable report includes definitions from sqlite_schema for tables, indexes, views, and triggers. SQLite-generated objects without separate SQL receive an explanatory line. Save report exports text. Schema names and definitions can still be private, so review a report before sharing it.

![The report shows the rotation table definition, including its slot number constraint and directory and date columns.](figures/analysis.png)

Choose and import a database copy first resolves pending admin changes. The source is opened read-only and SQLite's backup API creates a local candidate. Compatibility changes add required columns, establish a unique settings key, preserve extra tables, and fill required standard slots where needed. Wide rotation data is renamed to a preserved backup and converted to the slot layout. The import stops for review when it finds multiple settings rows. Legacy time normalization applies to the candidate.

A consistent ZIP of the current local database precedes replacement. The app validates the candidate's songs, silence, and settings, releases active playback, replaces the local database, and reloads. If replacement or subsequent initialization fails after the local backup is taken, it can restore the prior snapshot. The final conversion report records the snapshot path and normalized values count. Copy the music separately and verify the corresponding paths under the destination executable.

## Recovery and backups

Close the copy being restored and make a separate ZIP of the current database. Confirm the exact destination and its drive letter. Extract carillon.db from the chosen backup into the installation's databases directory. In-place startup backups are beside the database under databases. The Database import action keeps its local backups under databases\backup. Reopen in Evaluation mode and verify settings, paths, and upcoming events before resuming normal on-site operation.

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

Open CarillonBells.sln in a Windows IDE with .NET 10 and WPF support. CarillonBells.csproj targets net10.0-windows. Directory.Build.props routes project intermediates into dcu and excludes local output, tools, and private directories from compilation. The executable icon, splash and clock assets are WPF resources. In the single-executable release, local HTML Help is embedded so upgrades can use it immediately. The new USB package also includes a Help folder for direct access.

Run tools\build.ps1 with Release or Debug. For the current USB release, publish a 64-bit build with -Runtime win-x64, -Publish, and -SingleExe. The script uses tools\dotnet when installed, otherwise it uses the system dotnet executable, and directs CLI settings and NuGet packages into project-local caches. The lock file and project file identify dependency versions. The bundled EXE contains the 64-bit .NET Desktop runtime and the SQLite native component.

Use an approved pre-change ZIP before updating a working installation. Close the app and validate the application build first. The -PortableRoot option applies to an initialized v9 destination on the multi-file publish path. It copies and hash-verifies application files while preserving the database, audio, logs, and data directory. For the single-executable USB release, extract the Upgrade ZIP to the existing installation folder. To relocate a working installation, close it and copy the complete folder.

## Validation and packaging

Run CarillonBells.exe --self-test on disposable data before deployment and on each updated installation. Verify the generated regression-results.txt and failure count alongside the process exit code. Run --audio-test followed by a local reference MP3 to test decoding, quiet WAV playback, serial repeats, pause and stop, dispatch with Clock open, time discontinuity, silence, endpoint mute, and cleanup. Audio tests exercise actual Windows audio, so run them in a safe test setting. Review audio-regression-results.txt. Exercise exact layouts, deployed resources, save and cancel, database import and restoration, and portability under a different working directory when those surfaces change.

Package the 64-bit USB release as separate New Install and Upgrade ZIPs. The New Install ZIP contains CarillonBells.exe plus data, databases, Documentation, Help, logs, and Music. Verify that each scheduled starter recording resolves inside Music and that the starter database contains only the intended time-marking rows. The Upgrade ZIP contains the executable and Documentation for extraction beside the existing database. Review archive contents and hashes. Keep private working databases, credentials, music libraries, diagnostic logs, backups, and scratch output out of the distribution. Record physical endpoint removal, USB disconnect behavior, real SMTP delivery, diverse monitor scaling, and long-running operation as separate deployment evidence when checked.

## Maintain the guides

The editable manuscripts are under docs\guides\source. Run tools\build_guides.py with an output folder to generate the DOCX drafts. In LibreOffice Writer, update each table of contents, save the DOCX, and export the PDF from that document. Check the dotted leaders, page numbers, and links after updating the contents. The USB package collects the finished DOCX guides from docs\guides and their PDFs from docs\pdf into Documentation.
