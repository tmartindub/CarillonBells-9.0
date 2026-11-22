# FMX2CSharp - Complete Carillon Migration Handoff

Prepared: November 21, 2026
Product: Westminster Chimes and Carillon Bells
Source: Delphi FMX version 8.1
Target direction: C# / Windows Presentation Foundation (WPF)
Status: Main-screen design prototype approved; full conversion has NOT started.

## 1. Read This First

This document is the handoff to Codex in a NEW session. Read the user's current
project directives before doing anything. State that you understand them.

The user explicitly wants ALL functionality of the existing Delphi application
in the C# application, including the built-in CLOCK. This is a complete
application conversion, not a mockup, a partial rewrite, or a simplified player.

The user liked the separate WPF main-screen prototype and approved using its
layout and overall visual direction. Preserve that direction as the starting
point. Complete the application behind it and add every missing page and feature.

SQLite is an explicit user requirement. Do not substitute SQL Server, JSON,
browser storage, an in-memory-only data store, or another database engine.

The handoff-preparation session was authorized only to write this handoff and
copy the specified database. It did not begin the full conversion. Continue the
implementation when the user starts the new session and instructs you to proceed.

Do not modify, rebuild, replace, or retire the working Delphi application as part
of initial C# development. The existing Delphi application is operational software
used internationally. Build and test the replacement separately.

Feature parity does not mean reproducing conversion-generated bugs, blocking
message loops, hardcoded screen coordinates, or the known Windows decoder
failure. Preserve intended behavior; record and discuss material behavior changes.

## 2. Exact Locations and Baselines

### Working Delphi source - inspect read-only

    C:\New Delphi Projects\Carillon8_1- Portable - FMX

Project:
    Carillon.dproj
Entry point:
    Carillon.dpr

Repository:
    https://github.com/tmartindub/Carillon8_1

Source HEAD observed while preparing this handoff:
    77c6252985d0ed7db68227bf80478fd692ef0a1c

Important earlier commits:
- c424f91: Fix preserving playlist groups, selection, and scroll during edits.
- c8126c1: Record the November 11 published program packages.
- 77c6252: Add the grid-editing fix to the app's website maintenance notes.

Recheck local status and remote HEAD before relying on those identities; later
user changes may supersede this snapshot. Never discard the user's changes.

### Approved separate WPF prototype

    C:\New Delphi Projects\Carillon WPF Prototype

Project:
    Carillon.WpfPrototype.csproj
UI:
    MainWindow.xaml
    App.xaml
Interaction/model:
    MainWindow.xaml.cs
    PrototypeModel.cs
Build setup:
    Directory.Build.props
Executable:
    bin\Release\Carillon.WpfPrototype.exe
Packaged preview:
    export\Carillon-WPF-Prototype.zip

Repository:
    https://github.com/tmartindub/Carillon-WPF-Prototype

Prototype HEAD observed:
    941654ed71543d56719fe6fd5e0ac55eeebf9adb

The prototype is a reference deliverable. Preserve its approved baseline.
Recommended production destination, NOT created by the handoff session:

    C:\New Delphi Projects\Carillon CSharp

A separate production solution is preferable to silently turning the approved
preview into an unstable development build. Reuse its XAML visual design and
appropriate code deliberately. Choose/confirm the production name in the new
session if necessary. Do not put C# implementation files into the Delphi folder.

### This handoff

    C:\Downloads\FMX2CSharp.md

### User-authorized database copy - PRIVATE

User specified source:
    F:\databases\carillon.db

Verified local copy:
    C:\Downloads\FMX2CSharp-data\databases\carillon.db

Size:
    192512 bytes

SHA-256 at copy:
    4F7C94216CC4F403C6AAC310AE4645851597EABC056798C6A87796C0444C09BE

The user closed Carillon on F: before copying. The source hash was checked before
and after copying; the destination matched both. No SQLite WAL/SHM sidecar files
were present. PRAGMA quick_check returned "ok" on an in-memory inspection of the
copied bytes. No SQL writes were made to either source or copy.

This is a PRIVATE reference baseline, not a sanitized redistributable database.
It may contain working SMTP credentials, recipients, organization details, or
other configuration. Never print those values, commit them, upload the database,
include it in a public package, or send test email without authorization.

User instruction: COPY ONLY. Do not change F:\databases\carillon.db.
Keep the verified local copy intact too; make a separate development working
copy before any migrations or writable C# testing. Do not make the current WPF
demo automatically open this database.

Do not assume F: is the permanent drive letter. Portable deployment is a core
requirement; installed drives have changed repeatedly during this project.

## 3. User Directives That Matter Here

Re-read the latest AGENTS.md and user instructions in the new session; this
summary supplements rather than replaces them.

- Obtain authorization before changes; when proceeding, state intended work and
  make the required pre-change backup. No post-change backup unless requested.
- The current physical backup drive is H:. It is a USB disk and may take up to
  60 seconds to wake. Do not report a disk failure just because first access is
  slow or initially does not enumerate.
- All physical backups must be ZIP files. Do not create uncompressed folder
  copies as backups. Use a clearly named matching project backup folder on H:.
- Git locally and GitHub are authoritative backup sources. Check project status
  and remote state before starting. Commit only intended validated changes.
  Routine pushes are approved, to applicable public/private remotes.
- Ask before a force-push, rewriting published history, or overwriting unreviewed
  remote work. Preserve unrelated dirty changes.
- Public GitHub repository creation for the project has standing authorization.
  Keep real databases, secrets, logs, dumps, and personal test files out of it.
- Use the account's GitHub no-reply email for commits. A previous normal-email
  commit was rejected by GitHub's private-email protection. Do not disable that
  protection or change global identity settings to work around it.
- UI forms must remain editable in the IDE. Declare controls, layout, sizes,
  colors/styles and templates in XAML. No runtime-only control construction.
  Bound data, events and transformations are normal application behavior.
- Use clear, specific project names for classes, variables and fields.
- Prefer the simplest correct implementation. Do not add miscellaneous helper
  code, speculative abstraction frameworks, or unnecessary packages.
- All requested features must work; do not leave labeled-but-inert pages.
- Read-only analysis means no edits, builds, database migrations or artifacts.
- Deep dives can involve up to five passes; stop sooner when the cause is clear.
- Keep temporary test projects, verification downloads and disposable outputs
  out of the finished project, and clean up artifacts you created.
- Do not close the IDE, editors, browsers or unrelated applications. The user
  permits closing a project app that blocks a rebuild only when unsaved work is
  verified safe. Always identify which app you closed.
- Use LibreOffice, NOT Word, if office-document production is later requested.
- Requested Engineering and User Guides must have DOCX and PDF companions,
  title page, last-changed date and a valid updatable table of contents.
- Requested help is HTML, with GTranslate floating-widget translation support.
- Standard UI direction is orange and blue, consistent with the approved preview.

Preferred project folders:

    bin
    dcu
    docs\guides
    docs\pdf
    export
    help
    images and icons
    samples
    source distributions
    tools

In C#, route compiler intermediates into dcu or another explicitly agreed
location. The prototype uses Directory.Build.props so the intermediate path is
set before MSBuild imports; setting it late caused MSB3539.

Tool locations:
- .NET SDK: C:\Program Files\dotnet\dotnet.exe
- Git: C:\Program Files\Git
- GitHub CLI used successfully: C:\Program Files\GitHub CLI\gh.exe
- Python: C:\Users\marti\AppData\Local\Microsoft\WindowsApps\python.exe
- Python verified previously: 3.13.14, 64-bit. Do not rely on plain python/PATH.
- LibreOffice: C:\Program Files\LibreOffice\program\soffice.exe
- LibreOffice CLI: C:\Program Files\LibreOffice\program\soffice.com
- Delphi tools, reference builds only if separately authorized:
  C:\Program Files (x86)\Embarcadero\Studio\37.0\bin
- Delphi environment: rsvars.bat in that directory.
- Delphi compiler: dcc32.exe in that directory.
- Delphi MSBuild must be elevated for its environment file access.

## 4. What the Approved WPF Prototype Actually Does

It is a design study with in-memory sample data, NOT the migrated application.

Implemented:
- Blue sidebar, orange actions, light cards, grouped music list and schedule editor.
- Sample song selection, grouped expand/collapse and text filtering.
- From/Until date controls, seven weekdays, repeat text, twelve time entries.
- Save and Cancel on sample records, preserving group expansion and selection.
- First/Previous/Next/Last navigation.
- Sample schedule overlay, simulated enable/pause, two color treatments.
- Three-second synthesized PCM bell preview, pause/resume, stop, volume and mute.
- Whole-design scaling via Viewbox at restored and maximized sizes.
- All visible controls declared in MainWindow.xaml and App.xaml.
- Successful Release build with zero errors and zero warnings.
- Screen inspection exercised Save, Cancel, sample schedule, theme selection,
  bell playback, normal size and maximized size.

NOT implemented:
- Persistent SQLite access, real playlist CRUD, actual selected-file playback.
- A real scheduler or startup/daily maintenance services.
- System endpoint volume/mute and Windows-tray synchronization.
- The legacy or weekly silence engines.
- Groups, random-directory, System, Email, Clock, Log or Help pages.
- Reports, SMTP delivery, real uptime/health telemetry.
- Single-instance lock, USB keepalive, power-hold, production packaging.

Sidebar text for unimplemented pages is just text. The production application
must make these real. Add Clock, Log and Help navigation explicitly.

The player deliberately generates a PCM tone. It does NOT prove MP3 decoding
works, nor solve the Windows decoder problem.

Two prototype details must not become accidental product requirements:
1. Its repeat validation currently permits 1..12; Delphi playback clamps repeats
   to 1..5. Twelve schedule fields and twelve silence rows are separate limits.
2. Its next-bell/silence overview is illustrative. Replace all demo values with
   real computed state, and remove "demo" labels only as each feature becomes real.

## 5. Source Inventory and Migration Map

Do a full source/form/menu/timer inventory before implementation. This map is
a starting point, not permission to skip code that is not listed.

| Delphi source | Responsibilities to preserve | C# direction |
|---|---|---|
| Carillon.dpr | Exclusive executable-relative lock, startup/splash, create forms, run/exit | Application bootstrap, instance guard, lifetime |
| Playlist.pas / Playlist.fmx | Main window, playlist CRUD, scheduling, media playback, grouped grid, status, menus, portability, shutdown | Main window/view model and focused domain services |
| ScheduleManager.pas | Build/sort/deduplicate schedule, weekdays, ranges, silence predicate, full-day report mode | Deterministic schedule builder |
| SilenceManager.pas | 12 weekly windows, overnight rules, prior mute restoration, logging events | Recurring silence service |
| SilenceScheduleForm.pas / .fmx | Edit/navigate/save weekly windows and Purpose | Real silence editor |
| AudioManager.pas / MMDevAPI.pas | Endpoint volume, mute, callbacks and slider synchronization | Windows Core Audio integration |
| SettingsForm.pas / .fmx | Organization/logging, liturgical/funeral silence, date maintenance, log trim | System settings page plus application-owned maintenance |
| email.pas / .fmx | Email settings, SMTP, test actions, 05:00 health report | Email page, report builder, transport and report scheduling |
| GroupForm.pas / .fmx | Seasonal groups CRUD, dates/time, holiday recalculation | Real groups editor and calendar rules |
| RandomDirectory.pas / .fmx | Seven directory slots with annual dates, browsing, Save/Cancel | Real rotation editor with explicit navigation/save |
| RandomDirectoryManager.pas | Row-schema detection/migration, slot priority, annual matching | Rotation repository/resolver |
| ClockUnit.pas / .fmx | Embedded artwork, analog hands, one-second redraw, close | Full analog Clock window in XAML |
| LogManager.pas | Logging, timestamp compatibility, played-song counts, 30-day trimming | Log store/parser/count service |
| LogViewerForm.pas | Latest 1000 rows, newest first, timestamp/event columns, Refresh/Close | XAML log viewer |
| EasterCalculator.pas | Gregorian Easter calculation | Calendar rules with reference tests |
| PowerHoldSimple.pas | Prevent system sleep while active; release on exit | Windows power lifetime integration |
| CarillonTheme.pas | Shared palettes, persistence, portable SQLite path, stable settings key | Shared WPF resource dictionaries and DB configuration |
| CarillonSplash.pas / .fmx | Startup identity, splash timing/artwork/version | Splash/bootstrap presentation |
| Overlay.pas / .fmx | Auxiliary overlay/centering used by optional actions | Preserve needed interaction, avoid unnecessary window machinery |
| Conversion program\DBUpgrade724To81Main.pas / .fmx | 7.2.4 upgrade analysis, backup, schema conversion, time normalization, reports | Equivalent supported migration workflow/utility |
| tools\ReportRegressionTests.dpr | Existing schedule/count regression cases | Port domain regression scenarios |
| Help and docs folders | User-facing explanations and terminology | Preserve access; update for C# when authorized |

Read both .pas and .fmx: controls may be wired in either FormCreate or the form.
Do not port VCL-to-FMX generated binding/style emulation mechanically. Rebuild
the equivalent behavior using normal WPF binding, commands and resource styles.

## 6. Main Window and Playlist Feature Parity

Required main-screen capabilities:
- Show actual organization/product identity, current schedule status and uptime.
- Group songs by season/group with song counts and total group duration.
- Open All/Close All; useful scrollbars without the old extraneous white strip.
- Readable selected rows in every palette, including inactive/focus changes.
- Selecting a song shows that record's actual schedule, not stale form data.
- Edit From/Until inclusive date range, Mon-Sun flags, scheduled_time1..12,
  group/season and playback repeat count.
- Add one or multiple selected audio files; existing chooser offers MP3 and WAV.
- Calculate duration correctly; store compatible duration text.
- Detect duplicate playlist entries using the actual database composite key.
- Delete with appropriate confirmation and preserve a sensible nearby selection.
- Save/Cancel explicitly; confirm pending edits on closing/navigation as required.
- First/Prior/Next/Last, Add/Delete/Edit/Save/Cancel/Refresh equivalents to the
  existing navigator; do not rely on moving focus to silently commit records.
- Manual Play/Pause/Resume/Stop, repeats, current title and remaining countdown.
- Show last song played/time, playlist size, songs played today and version.
- Schedule enable/disable and current/remaining schedule displays.
- Working navigation to Groups, Clock, System, Email/Reports, Themes,
  Random Music, Silence Schedule, Log, Help and Exit.
- Exit using either X or File/Exit with the same orderly cleanup.

Mandatory November grid fix:
- Starting a date/time edit can pause playback without resetting the group map.
- Save and Cancel must keep the group open, the selected song visible and the
  scroll position stable.
- Preserve selection by stable key after a query refresh; not by row number or a
  bookmark invalidated by closing the dataset.
- Sorting/filtering changes must not save into the wrong record.
- Do not reset to startup-collapse state on an automatic schedule pause/resume.

Source references: BeginPlaylistEditSession, EndPlaylistEditSession,
RefreshGrid(APreserveSelection), chkEnableScheduleClick,
PlaylistQueryAfterPost/AfterCancel and UpdatePlaylistFromSeasonalGroups.

Layout:
- Use the approved WPF composition, colors, hierarchy and separation of sections.
- Keep every form/control editable in XAML and accessible in Visual Studio.
- Use scalable layout/transforms/resources; avoid hardcoded monitor-specific hacks.
- Verify restored/maximized sizes and DPI scaling, not just one screenshot.
- The prototype Viewbox scales the whole design. A production adaptive Grid or
  other arrangement is acceptable if it preserves the approved appearance and
  usability at smaller sizes. Do not introduce overlap, clipping or offscreen buttons.

## 7. Scheduler - Behavioral Contract

Read ScheduleManager.pas and the Playlist callers together.

Current observed rules:
- A song needs non-null From/To dates and today's date within the date range.
- Weekday matching uses Monday=1 through Sunday=7.
- Each non-null scheduled_time1..12 creates a candidate; midnight is valid.
- Clamp playback repeats to 1..5, as the current Delphi builder does.
- Resolve relative song paths from the executable's directory.
- Deduplicate the same playlist record/path/time across duplicate time fields.
  Do not suppress different records merely because they share a timestamp.
- Sort by time, preserving deterministic order for simultaneous events.
- Full-day report builds can include past events; active playback builds do not.
- Preserve current selected record and grid state while compiling.
- Apply weekly-silence exclusions when building and recheck at dispatch.
- The current dispatcher runs on a one-second interval, removes a due event,
  and starts one event when no playback is in progress.
- Rebuild after enable/re-enable, relevant committed edits and daily maintenance.
- Current daily rebuilding occurs on first eligible tick after 00:05 once per day.
- Reset today's count/record yesterday's summary correctly across midnight.
- Handle startup, long-running operation, computer sleep/resume, date changes,
  manual clock changes and DST deliberately. Write down the policy for late
  events, repeated clock times and missed events; do not invent it silently.

Existing administration semantics:
- Opening Groups, System, Email, Random Directory or Silence editing disables
  the schedule. Closing those dialogs enables it and rebuilds it, even if it was
  disabled before opening. The user explicitly requested this behavior.
- Main playlist editing pauses only if previously enabled and resumes after
  Save/Cancel using its edit-session flag. Preserve that distinction.
- Viewing the clock or log must not suspend application background work.

Implementation requirement:
- Use one application-owned scheduling/lifetime model, cancellation and a
  serialized playback path; do not make page visibility own essential timers.
- Do not port Sleep/Application.ProcessMessages polling loops onto WPF's UI thread.
- Reports must build independent schedule snapshots, not consume the dispatch queue.
- Use stable record keys in scheduled events, not mutable grid row indexes.

## 8. TWO Silence Systems - Both Are Required

### 8.1 Recurring weekly silence windows

Source: SilenceManager.pas and SilenceScheduleForm.pas/.fmx.
Table: silence_schedule.

Requirements:
- Exactly 12 editable slots/records, IDs 1..12.
- Enabled, Purpose (TEXT/memo-backed description shown in an edit control),
  start time, end time and Monday-Sunday flags.
- First/Prior/Next/Last, Save, explicit cancel/unsaved-change handling and Close.
- Accept documented clock-time formats such as 10:00 AM and 14:05.
- Disabled windows stay available for reuse; do not remove rules just because a
  particular day's occurrence ended.
- Handle same-day and overnight windows and associate the after-midnight segment
  with the rule's starting weekday.
- Restore a preexisting manual/system mute correctly; do not always unmute.
- Handle overlapping/adjacent windows without a brief unwanted audible interval.
- Apply state immediately after startup and committed rule changes.
- Rebuild schedule when exiting the silence editor.
- Log silence starts and restoration, including Purpose.
- Remaining-schedule displays must filter expired occurrences.

Boundary detail that MUST be characterized:
- IsScheduleTimeSilenced passes AEndInclusive=True.
- IsTimeSilenced passes AEndInclusive=False.
- Thus existing compile-time suppression and live muting differ exactly at the
  end instant. Test this source behavior and agree on any policy change rather
  than assuming every interval is half-open.
- Characterize equal start/end and overlapping-window priority from source/UI.

### 8.2 Legacy liturgical and one-off funeral silence

Source: SettingsForm.pas, especially SilenceTimerTimer, LoadSilenceSettings,
SaveSilenceSettings, UpdateEasterDates, UpdateAllSoulsDate.

Required fields/behavior:
- Global enable for the legacy silence system.
- Ash Wednesday, Holy Thursday, Holy Saturday and All Souls dates.
- One-off funeral/other begin date/time and end date/time; clear/reset fields.
- Gregorian Easter-related dates: Ash Wednesday=Easter-46 days,
  Holy Thursday=Easter-3, Holy Saturday=Easter-1.
- Next-occurrence handling for Easter and November 2 All Souls.
- Legacy liturgical mute at 02:05 and unmute at 23:55 on the relevant dates.
- Funeral mute/unmute at the configured full date/time.
- Range validation; close/save behavior; log and email notifications.
- Source evaluates legacy silence every two seconds and guards repeated actions.
- Annual liturgical date maintenance runs after January 2 00:15, once/year
  according to the source guards; inspect startup handling carefully.

Do not omit this system because a newer weekly editor exists.
The existing legacy code invokes a toggle; characterize the intended result and
coordinate competing mute reasons so one reason cannot unmute another active
reason. Preserve features while avoiding blind toggle-induced state errors.
Tests must cover restarts during silence and more than one active silence reason.

## 9. Audio Playback and System Volume

The full application must play actual user MP3/WAV files, not the generated
preview tone. Support accurate duration reading, play, pause/resume, stop,
completion, repetition, sequencing, missing-file handling and UI responsiveness.

Known history:
- KB5124010 caused crashes while audio was loaded/played, across old and new
  Carillon versions. Removing the update restored operation in testing.
- Crash evidence identified msmpeg2ac3dec.dll, exception 0xC0000602.
- Installed Delphi FMX.Media.Win.pas uses DirectShow RenderFile.
- Simply assigning a filename/reading duration can enter the affected decoder.
- A C# rewrite using a Windows-dependent media component can reproduce the same
  failure. Changing language does not itself remove this dependency.
- Both playback AND duration lookup need an independent decoding path if that is
  the chosen mitigation.
- The website warning was removed November 21 at the user's request. This does
  not establish that the audio backend was replaced; it was NOT.
- Current Microsoft resolution/update status has not been checked for this
  handoff. Verify it if relevant; do not repeat old status as current fact.

Audio design decision is not yet implemented/approved in detail:
- Earlier ideas included BASS, libVLC, miniaudio and a small dedicated decoder.
- BASS is proprietary with noncommercial-use conditions. The user wants an
  open-source app and asked about compiling the audio engine into the EXE.
- Miniaudio was discussed as a permissively licensed candidate, but linking and
  interop were NOT proved by the WPF prototype.
- For C#, evaluate independent managed decoding plus Windows PCM output as well
  as native options. Verify actual current licenses, supported formats, bitness,
  dependencies and packaging before choosing. No package is mandated here.
- Do not silently adopt WPF MediaPlayer, Media Foundation, DirectShow or a wrapper
  that still delegates decoding to the failing Windows component.
- Single-file packaging and truly static native linking are different. Explain
  native DLL/extraction requirements accurately before promising a single EXE.
- Make a small isolated proof using MP3/WAV, repeated loads, duration reads,
  stop/restart and both target architectures before connecting the real scheduler.

System volume behavior is separate:
- Port endpoint-level master volume and mute using the Windows Core Audio APIs.
- Changes made in the Windows tray must update the application's slider/button.
- Clicking Mute/Unmute must change system mute, not merely redraw button text.
- Keep callback guards to avoid feedback loops and UI-thread deadlocks.
- Dispose/unregister callbacks on exit; consider output-device changes.
- The current slider is disabled while muted; inspect/preserve intended behavior.
- The prototype's slider changes only the generated tone. Replace that behavior
  with the real product semantics, not merely a relabeled preview slider.


## 10. Seasonal Groups and Calendar Calculations

Source: GroupForm.pas/.fmx, EasterCalculator.pas,
Playlist.UpdatePlaylistFromSeasonalGroups.

Required:
- Display and edit group name, From/To dates and play time.
- Add/Delete/Save/Cancel and clear navigation/selection behavior.
- Real editable controls; the old FMX selector grid used separate generated
  editors. WPF can provide a proper editable DataGrid or a consistent detail
  editor, but explicit posting/navigation must remain reliable.
- Manual "Recalculate Dates for Upcoming Year" action.
- Automatic annual group maintenance: source checks January 3, after 00:02,
  once/year according to FLastAutoRecalcYear.
- Preserve custom/unrecognized group names and manually supplied values.
- Sync seasonal settings into matching playlist records as the current app does.

Date rules verified in source:
- Christmas: December 22 through December 26.
- Christmas Eve: December 24.
- Easter: calculated next Easter date.
- Epiphany: January 6.
- Independence Day: July 4.
- Memorial Day: last Monday of May.
- New Year's Day: January 1.
- Nine Eleven: September 11.
- Thanksgiving: fourth Thursday in November.
- Valentine's Day: February 14.
- Veterans Day: November 11.

Do not replace the user's rules with a different liturgical or holiday convention
just because another convention seems more familiar. Test before/within/after a
date range and across New Year. Reinspect source for later added groups.

Group-to-playlist synchronization matters:
- Source excludes "Daily List".
- Matches trimmed season names case-insensitively.
- Copies group From/To dates and play time to scheduled_time1.
- Clears scheduled_time2..12 for those synchronized seasonal records.
- This can replace individual seasonal-song timing edits when schedule resumes.
  Preserve/characterize current behavior and surface any proposed change.
- Rebuild schedules and refresh the UI without losing the selected song.
- Use transactions and parameterized SQL.

## 11. Random Music Directory Rotation

Source: RandomDirectoryManager.pas and RandomDirectory.pas/.fmx.

There are SEVEN slots, not seven independent weekdays:
- Slot 1 is the default/fallback directory.
- Slots 2..7 are annual date-window overrides.
- Overrides are evaluated in slot_no order; first matching nonempty directory
  with both dates wins.
- Compare month/day for annual recurrence, including cross-year ranges such as
  December through January.
- When no override matches, use slot 1.
- Do not convert this into a weekday-rotation feature by assumption.

Required editor:
- Show default and six override directories.
- Browse/choose directories.
- Edit annual From/To dates with compatible validation.
- Explicit Save/Cancel and navigator-like behavior.
- Unsaved-change confirmation on Close.
- Do not depend on changing focus to silently post; the user disliked that old
  interaction and specifically requested dependable explicit posting.
- Theme the whole page immediately when themes change.

Playback integration:
- Existing placeholder titles contain "Random Music Generator".
- Resolve the active directory at playback time, then choose an MP3.
- Handle empty/missing/unavailable directories and unsupported/missing files.
- Preserve whatever relative/absolute path rules the existing application uses.
- Do not copy the user's entire song library as part of migration setup.
- Preserve random seed/selection behavior as appropriate; source reseeds during
  daily maintenance. Do not add nonrepetition guarantees unless requested.

Schema compatibility:
- Current table is row-based: slot_no, rand_music_dir, frm_date, to_date.
- Older source-root sample DBs still use a single wide row with dir1..dir7 fields.
- EnsureRandomDirectorySchema detects/migrates the wide representation and
  preserves PL_RAND_DIR_ROTATION_WIDE_BACKUP.
- The actual F: copy already has the row-based table AND the wide backup table.
- Preserve backup/unknown tables. Do not flatten or destroy them to simplify code.

## 12. System Settings, Background Maintenance and Portability

System page:
- Organization name.
- Logging on/off.
- Legacy silence enable and all dates/times described above.
- Recalculate Easter/All Souls.
- Reset funeral fields.
- Save All/Cancel All, navigator equivalents and pending-edit handling.

Maintenance must run while pages are hidden/closed:
- Log trim at startup.
- Daily log trim on first tick after 00:10, once/day, even if playback logging is off.
- Daily scheduling/played counts maintenance after 00:05.
- Annual settings date maintenance after January 2 00:15.
- Annual seasonal-group recalculation on January 3 after 00:02.
- Automatic morning report at/after 05:00 under current daily-send guards.
- Recurring/legacy silence evaluation.
- Uptime/status refresh.
- USB keepalive: current main form writes data\keepalive.txt every 30 seconds.
- Prevent automatic system sleep while the app is active, using an equivalent of
  SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED); release on exit.

The current Delphi app auto-creates Settings, Email, Groups, Clock and other
forms in Carillon.dpr. Their timers can run without showing the form. This was
a source of confusion in earlier debugging. Do not lose background functionality
when implementing WPF pages lazily.

Portable paths:
- Runtime DB: executable directory + databases\carillon.db.
- Logs: executable directory + logs\CarillonPlayLog.txt.
- Help/assets/configuration must use the appropriate portable base too.
- Never use Environment.CurrentDirectory as the application data root; a launch
  script/shortcut can set a different working directory.
- Do not hardcode F:, E:, D:, C:\Downloads or the development folder in production.
- Support stored .\ paths and compatible absolute external paths.
- Avoid a destructive global replace of drive letters. Normalize path handling
  carefully and preserve user-selected paths outside the portable folder.
- Show clear errors for missing/read-only/disconnected portable storage.
- Prevent simultaneous Delphi/C# use of the SAME database during migration tests.
- An exclusive executable-relative Carillon.lock currently prevents duplicate
  instances from the same portable folder. Preserve the appropriate semantics,
  releasing the lock on normal exit and after crashes.

## 13. Email Settings, Notifications and Morning Health Report

Source: email.pas/.fmx and Playlist.SendToEmail.

Required settings:
- Email enabled.
- Username, password/app password, SMTP host and port.
- Main alert recipients, test recipient, morning-report recipients.
- Show/hide password.
- Save/Cancel, Close with pending-change handling.
- Send Test Message.
- Test Morning Report.
- Preserve from_address compatibility where present; source currently sends from
  configured username in its common alert path. Do not silently discard fields.

Transport:
- Current Delphi path uses Indy/OpenSSL, TLS 1.2 and explicit STARTTLS.
- A suitable maintained C# transport may remove those old OpenSSL DLLs, but choose
  deliberately and document any SMTP/security behavior differences.
- Validate configuration, report DNS/authentication/TLS/network failures clearly.
- Never freeze the UI or interrupt scheduled audio while sending/reporting.
- Never send real messages during automated tests without explicit permission.
- Do not announce success if the underlying transport failed.
- The earlier intermittent "Host not found"/10001 report was left for observation,
  not established as an application defect requiring an unrelated fix.

Daily reporting:
- Source timer interval is 60 seconds and triggers at/after 05:00 once/day.
- Startup after 05:00 initializes LastReportDate to today, preventing a surprise
  catch-up send. Reproduce/confirm that intentional behavior.
- Test Morning Report remains available when email is enabled.
- Make sending independent of whether the Email page is visible.
- Preserve general alerts, including legacy silence notifications.

Report sections to carry over:
- Date/time and organization.
- Portable/system drive identity and disk usage.
- Last Windows boot and Windows uptime.
- CPU and RAM usage.
- Application version, last startup and application uptime.
- Audio device/backend information: report the actual backend, not old hardcoded text.
- Total scheduled events today, events remaining, schedule enabled/paused.
- First/last/next event.
- Recent play activity: last 20.
- Yesterday's play summary.
- Daily maintenance activity: last 10.
- Playlist record count and total duration.
- Missing/broken song files.
- Previous-date errors.
- Overall health score and remaining schedule listing.
- HTML escaping for user data and filenames.

Previous report fixes MUST survive:
- Yesterday means the immediately preceding calendar date, not the newest old
  summary found anywhere in a log.
- Today's count survives restarts by counting today's recorded "Played Song: "
  lines. Do not reintroduce "Daily play count checkpoint" on every song.
- Old checkpoint entries are ignored when counting plays.
- Explain that plays made while logging was disabled cannot be reconstructed.
- Full-day report counts come from an independent schedule build even if editing
  Email temporarily disables the live queue.
- Silence exclusions apply to both report and playback schedule.
- Durations are intervals: m:ss means minutes:seconds; h:mm:ss means
  hours:minutes:seconds. Do not parse 1:30 first as a clock time of 1 hour 30 minutes.
- Total duration can exceed 24 hours; use a duration type, not a time of day.
- Count/report calculations must not disturb the selected playlist record.

## 14. Logs and Log Viewer

Source: LogManager.pas and LogViewerForm.pas.

Required:
- Append timestamped entries using yyyy-MM-dd HH:mm:ss - message.
- Read both that format and historical en-US date/time lines.
- Count only actual "Played Song: " messages for the requested calendar date.
- Preserve application startup, schedule enable/disable, playback, maintenance,
  silence start/restoration and meaningful error logs.
- Log according to the logging setting; trim independently of that setting.
- Retain the most recent 30 days by timestamps; compare carefully at the cutoff.
- Keep malformed/unparseable lines safe unless a deliberate policy is approved.
  Historical example 2/29/2026 is an invalid date; retaining an unparseable first
  line is not proof that an array loop skipped index zero.
- The viewer shows up to the latest 1000 entries, newest first, with Date/Time
  and Event columns plus Refresh and Close.
- Opening or refreshing logs must not block playback or change song selection.
- Synchronize concurrent append/read/trim and avoid corrupting a log during trim.
- No real logs in public source or release packages.

## 15. CLOCK - Explicit, Nonoptional Deliverable

The user specifically required the clock built into Delphi. Do not omit it,
replace it only with a text time display, or leave a "Clock" navigation placeholder.

Read both:
    ClockUnit.pas
    ClockUnit.fmx

Observed implementation:
- Separate Clock form, maximized by default, with a File > Close menu.
- Image1 fills the client area under the menu.
- ClockUnit.fmx embeds a 1680 x 864 PNG background in MultiResBitmap.
- The code saves/restores the background and draws hour and minute hands.
- Timer1 ticks every 1000 ms and reads the current system time.
- Minute angle = 360 * minute / 60.
- Hour angle = 360 * ((hour mod 12) / 12) + 30 * minute / 60.
- Hands are black, nominal thickness 10, minute length 217, hour length 170
  in the existing bitmap-coordinate implementation, with a pivot.
- The source increments a seconds counter but does not draw a second hand.
- There are hardcoded monitor-resolution offsets. These are implementation
  workarounds, not desired behavior to copy into WPF.
- File > Close closes the clock and returns to the application.
- Showing the clock must not pause scheduled bells or background services.

C# implementation direction:
- Preserve the recognizable clock artwork and working analog display.
- Extract the embedded artwork into the new project's images and icons assets;
  read from the Delphi file without modifying it.
- Declare clock elements, image, hands, pivot, transforms and menu in XAML.
- Bind hand angles to current time or update declared RotateTransforms.
- Use a coherent view coordinate system, correct pivot and proportional scaling.
- Center properly at different sizes/DPI; no monitor-resolution lookup table.
- Ensure correct 12-hour geometry, transitions at 11:59 -> 12:00, midnight,
  restore/maximize and return to the main window.
- Do not assume a digital label alone meets this requirement.
- An added second hand or a redesigned clock face is optional, not a substitute
  for parity, and requires user agreement if it materially changes the appearance.
- Release the timer/window resources on close/exit; ensure reopening works.

Clock acceptance requires a visible live ticking analog clock AND scheduled
playback continuing while that clock is displayed.

## 16. Themes, Visual Assets, Splash and Help

Current Delphi themes:
1. Cobalt Harbor
2. Royal Current
3. Emerald Glass
4. Copper Dawn
5. Indigo Velvet
6. Cranberry Silk
7. Atlantic Teal
8. Midnight Copper
9. Pine Gold
10. Graphite Ice
11. Verdant Slate
Plus Current: Custom Saved Colors.

The prototype's Porcelain Blue and Warm Parchment do not replace this full catalog.
Carry over equivalent palette choices and custom persisted colors while retaining
the approved blue/orange default direction. Read CarillonTheme.pas for exact
legacy color conversion and palette semantics.

Requirements:
- Shared resource dictionaries/palette service.
- Immediate updates across all open/newly opened pages.
- Correct text/background contrast, including selected rows.
- Single clean group-box borders; caption text matches intended theme color.
- No caption overlap, stray characters, old border fragments or selection washout.
- Theme persistence across restart using compatible settings or an agreed migration.
- Preserve icon/taskbar identity and splash/branding. The old taskbar icon was
  previously lost after a build; explicitly validate icons in shipped binaries.
- Clock artwork must survive extraction and packaging.

Help:
- Preserve access to the Help menu and existing user-facing topics.
- Source help is under Help\, including index.html, glossary, keyword index,
  manual print page, search index, table of contents and related assets.
- Updated help, when implemented/requested, should be HTML and support GTranslate.
- Translation helper convention: floating widget, product-name notranslate /
  translate=no protection, local assets/translation-selector.js with bilingual
  labels and alphabetic language ordering.
- Do not rely on an internet translation widget for basic local help availability.
- Existing Engineering and User Guides live under Delphi docs\guides and docs\pdf;
  consult them as supplementary evidence, not a replacement for code inspection.

## 17. SQLite Compatibility and Private Data

The F: database copy is the primary reference, ahead of old root/sample/package DBs.
Inspect its schema read-only first, then work on another copy.

Observed tables in the verified F: copy:

### playlist

Composite primary key:
    playlist_name TEXT
    song_name TEXT

Other fields:
    season TEXT
    play_date_from DATE
    play_date_to DATE
    scheduled_time1 through scheduled_time12 TIME
    play_monday INTEGER
    play_tuesday INTEGER
    play_wednesday INTEGER
    play_thursday INTEGER
    play_friday INTEGER
    play_saturday INTEGER
    play_sunday INTEGER
    num_times_to_play INTEGER
    song_duration TEXT

### seasonalgroups

    SEASONALGROUP TEXT PRIMARY KEY
    PLAY_DATE_FROM DATE
    PLAY_DATE_TO DATE
    PLAY_TIME TIME

### PL_SETTINGS

    ORGANIZATION_NAME TEXT
    RAND_SONGS_DIRECTORY TEXT
    LOGGING_STATUS INTEGER
    form_bgcolor TEXT
    form_fontcolor TEXT
    all_souls_day DATE
    other_silence_begin_date DATE
    other_silence_end_date DATE
    silence_enabled INTEGER
    ash_wed_begin DATE
    ash_wed_end DATE
    holy_thu_begin DATE
    holy_sat_end DATE
    other_silence_begin_time TIME
    other_silence_end_time TIME
    email_enabled INTEGER
    email_recipients TEXT
    email_user TEXT
    email_password TEXT
    smtp_host TEXT
    smtp_port INTEGER
    from_address TEXT
    test_recipient TEXT
    morning_report_recipients TEXT
    settings_id INTEGER

The stable settings row uses settings_id=1. The legacy fix adds/normalizes that
column and creates a unique index. Preserve this protection and check actual
indexes/constraints rather than assuming every database version is identical.

### PL_RAND_DIR_ROTATION

    slot_no INTEGER PRIMARY KEY
    rand_music_dir TEXT
    frm_date DATE
    to_date DATE

Slots 1..7.

### PL_RAND_DIR_ROTATION_WIDE_BACKUP

Legacy wide backup with:
    id INTEGER PRIMARY KEY
    rand_music_dir1..7 TEXT
    frm_date1..7 DATE
    to_date1..7 DATE

Preserve it; it is not the active row-based configuration.

### silence_schedule

    silence_id INTEGER PRIMARY KEY
    enabled INTEGER
    Purpose TEXT
    start_time TEXT
    end_time TEXT
    monday INTEGER
    tuesday INTEGER
    wednesday INTEGER
    thursday INTEGER
    friday INTEGER
    saturday INTEGER
    sunday INTEGER

Slots 1..12. Preserve actual constraints/defaults from the schema.

Database work requirements:
- Prefer retaining the existing schema and data representation initially.
- Use parameterized SQL, explicit transactions and stable primary keys.
- Microsoft.Data.Sqlite is a reasonable provider candidate; confirm supported
  runtime, native SQLite deployment and architecture dependencies.
- Do not require Entity Framework or an ORM merely to access these five domains.
- Preserve NULL versus midnight, optional dates and text-duration semantics.
- SQLite affinity does not guarantee uniform physical date/time storage. Examine
  actual storage types and existing normalizers before writing a serializer.
- Consider ISO dates/times and legacy Delphi/SQLite formats explicitly.
- System and Email share PL_SETTINGS. Save only owned fields against settings_id;
  do not write stale copies of unrelated settings back over another page's changes.
- Reproduce the fixed System-save then Email-save workflow without "zero records
  updated when one expected" errors.
- Support older wide random-directory DBs and stable-key upgrades only through a
  backed-up, transactional, intentional migration path.
- Preserve custom tables/indexes and existing schedules/settings.
- Never alter production F: or the verified baseline during development.
- Keep private databases outside public Git. Add ignore rules for databases,
  WAL/SHM/journal files, credentials, logs and dumps BEFORE copying private working
  data into any repository directory.
- Use a deliberately sanitized sample for releases. Do not assume a database is
  safe simply because email is disabled.

## 18. Application Architecture - Keep It Focused

Use WPF and C#. Keep all visual controls declared in XAML. Separate business
behavior from form lifetime so background work continues without visible pages.

A reasonable compact structure:
- Application bootstrap/lifetime and portable-path configuration.
- Domain records and a small SQLite repository/data-access layer.
- Main shell and page/window view models.
- Schedule builder/dispatcher.
- Audio playback and independent duration reader.
- Silence coordination for recurring, legacy and manual mute reasons.
- Calendar/group calculations and random-directory resolution.
- Logging/counts.
- Email transport/report construction.
- Theme resources.
- Clock view/view model.

These are direct product responsibilities, not a request to introduce a large
framework, generic helper library or dozens of empty projects.

Implementation guidelines:
- Use async operations and cancellation for work that can block.
- Serialize playback and application shutdown appropriately.
- Marshal Core Audio and worker callbacks to the UI without synchronous deadlocks.
- Keep SQLite connections/transactions short and ownership clear.
- Use event-driven playback completion rather than UI polling loops.
- Make a scheduler controllable by an injected/testable time source.
- Record explicit enabled/paused/silenced/playing state; do not infer it only from
  checkbox text or a visible window.
- Keep record keys separate from displayed row positions.
- Use coherent validation for typed dates, times and durations.
- Handle errors visibly and log them without crashing or swallowing every exception.
- Prefer dedicated application services over tying jobs to Settings/Email timers.
- Dispose playback, endpoint callbacks, timers, DB connections, locks and power
  holds exactly once during exit, including X and File/Exit.
- Do not add a Windows service, web server, browser UI or cloud dependency unless
  the user separately approves it.

## 19. Runtime, Builds and Release Packaging

Installed during prototype work:
    .NET SDK 9.0.321
    .NET Windows Desktop runtime 9.0.23
    Prototype TargetFramework: net9.0-windows

Recheck at the start of production work. Select a currently supported runtime,
preferably an appropriate supported LTS release, rather than treating the
prototype's SDK as a permanent product decision. Do not install/upgrade tooling
silently if authorization is required.

Required build/deployment evaluation:
- Windows-only native desktop WPF application.
- Preserve portable operation; no database in AppData or Program Files by default.
- Produce and test win-x86 and win-x64 builds as appropriate to existing users,
  with Debug and Release configuration support.
- Validate SQLite/audio/native dependencies match each architecture.
- Evaluate self-contained deployment so end users do not need a separate .NET
  installation. State clearly if framework-dependent builds are provided.
- A single-file publish may still extract native dependencies. Do not claim
  "everything compiled into the EXE" without proving the actual result.
- No administrator requirement for ordinary portable operation.
- Correct file version, executable icon and taskbar icon.
- Launch scripts must locate the executable relative to themselves.
- Run after extraction to a different USB drive letter and from a shortcut with
  a different working directory.
- Start with sanitized test data; validate all package contents and hashes.
- Maintain a release manifest identifying source commit and program/package hashes.
- Download public release assets again and verify them before declaring completion.

Existing Delphi releases are production artifacts, NOT a C# publication target:
    https://github.com/tmartindub/Carillon8_1/releases/tag/v8.1.0

Current download names:
    CarillonBells_8_1_Portable_Full.zip
    CarillonBells_7_2_4_to_8_1_Upgrade.zip
    CarillonBells_8_1_Executable_Update.zip

Their November 11 update includes the tested grid-edit fix.
Release manifest reference:
    source distributions\2026-11-11-program-update\release-manifest.json

Do not overwrite those Delphi downloads or repoint the public website to an
incomplete C# rewrite. A C# release needs its own approved release/version/channel
and a user-tested migration plan. Source pushes are not the same as approving a
replacement production release.

Packaging history:
- Songs and old_Songs folders were mistakenly included previously.
- They were intentionally removed from the corrected full deployment ZIP.
- Do NOT reintroduce either folder or the user's music library by rebuilding
  packages from stale local staging folders.
- Keep user database/settings/music when updating an existing installation.
- A database converter may be needed for legacy 7.2.4 compatibility, but the C#
  app must directly support the valid existing 8.1 database baseline.

Website repo, separate from application:
    https://github.com/carillon-bells/carillon-bells.github.io
Local:
    C:\New Delphi Projects\GitHub Pages

Latest observed website change:
- Windows KB5124010 notice removed November 21 by explicit request.
- Maintenance section includes the grid edit/Save/Cancel fix.
- Heading dated November 11, 2026.
- Application and website are separate repositories; never update the wrong page.

## 20. Licensing Discrepancy - Resolve Before Production Publication

The user stated that the application is free/open-source under Apache License 2.0.

Actual files observed:
- Delphi LICENSE begins "MIT License".
- Delphi README also says MIT.
- Prototype README says Apache License 2.0.
- Prototype LICENSE is MIT, copied from the Delphi project during preview setup.

Do not perpetuate this inconsistency or claim a legal conclusion from the
README alone. Bring it to the user's attention at the appropriate release/
project-setup decision and align the new project with their intended license,
preserving valid attribution and third-party terms.

Check licenses for the chosen decoder, audio output, SQLite provider, SMTP
library, icons and artwork. Do not introduce a proprietary/noncommercial-only
dependency while describing the whole application as unrestricted Apache code.

This handoff makes no changes to existing license files.

## 21. Suggested Implementation Sequence

Complete the full objective through tested milestones. Do not stop permanently
after rebuilding only the attractive main screen.

1. Read instructions, this handoff, source/forms and existing guides; check Git.
   Verify DB copy hash and inspect schema without changing it.
2. Establish the separate production project/repository and ZIP pre-change
   backups as appropriate. Preserve the approved prototype and working Delphi.
3. Build a feature-parity checklist covering every menu, form, timer, field,
   report, file and auxiliary utility. Record decisions needing user input.
4. Resolve runtime/deployment/audio/licensing choices with concrete evidence.
   Prove independent audio loading/duration/playback before relying on it.
5. Build portable SQLite access on a disposable development copy; test CRUD,
   NULL/date/time handling and concurrent settings pages.
6. Adapt approved main-screen XAML to real data, including full navigator actions,
   robust Save/Cancel, grouping, scroll/selection preservation and real status.
7. Implement calendar rules, seasonal groups, seven random directories and
   twelve recurring silence windows.
8. Implement application-owned scheduling, playback, system volume/mute and
   coordination of both silence systems.
9. Implement System and Email pages, background maintenance, logs/counts and
   morning health reporting.
10. Implement the real analog Clock, splash/icons, all themes, log viewer and help.
11. Implement/validate any needed legacy database conversion workflow.
12. Test parity and long-running operation, produce x86/x64 build/package outputs,
    update requested guides/help when appropriate, and give the user a safe
    separate executable to evaluate before any production promotion.

Use the existing behavior as the baseline, with separately approved changes
where old behavior is defective or ambiguous. Do not quietly remove features
because they are difficult to port.

## 22. Acceptance Matrix - Completion Gate

Record actual results for each item. "Control exists" is not "feature works."

### Data and editing
- Existing F: database copy opens; original/baseline hashes stay unchanged.
- Startup never switches to a different empty sample database.
- Composite playlist keys identify the correct song.
- Add single/multiple MP3/WAV files; duplicate detection and duration work.
- Edit all twelve play times, dates, weekdays, group and repeats.
- Save/Cancel preserve the selected song, group state and scroll position.
- Delete, Refresh and navigation do not post into the wrong record.
- Pending edits are handled when closing/navigating.
- Save System first, then Email, then System again: no stale-update conflict.
- Both older wide and current row-based rotation databases are handled safely.
- Unknown/backup tables and user data survive migrations.

### Schedule/calendar
- All weekdays, inclusive date ranges, midnight and 23:59:59.
- NULL time is absent; 00:00:00 is a real event.
- Duplicate slots do not duplicate the same event; simultaneous distinct songs
  remain distinct and deterministic.
- Repeat limits match 1..5, not the prototype's 1..12.
- Enable/disable and all administrator-screen pause/resume semantics.
- No duplicate dispatch or overlap during repeated enable/disable.
- Events around midnight and daily 00:05 rebuild.
- January 2 settings and January 3 group maintenance.
- Leap year, cross-year seasonal windows and next holiday calculations.
- Sleep/resume, clock changes and DST have documented tested behavior.
- Report schedule remains correct when live queue is paused.

### Silence
- Twelve slots, Purpose and all weekday flags save/reload.
- Same-day, overnight, overlap and adjacent windows.
- Exact start/end boundary behavior, including compile/live difference.
- Startup or edit during an active window.
- Silence start/end log entries and correct restoration.
- Manual mute before/during a silence window is respected.
- Legacy Ash Wednesday/Holy Week/All Souls/funeral dates/times work.
- Multiple simultaneous silence reasons do not unmute each other.
- Past occurrences disappear from Remaining Schedule without deleting rules.

### Playback and Windows integration
- Real MP3/WAV play, duration, pause/resume, stop, repeats and next event.
- Repeated load/unload/duration cycles without the old decoder failure.
- Missing/bad files and disconnected audio device fail gracefully.
- Manual and scheduled playback coordinate safely.
- Windows tray volume/mute changes update app controls and vice versa.
- Audio callbacks do not hang the UI during close or device changes.
- System power hold and release work.
- Same-folder second-instance prevention and lock release work.

### Reporting/logging
- Current-day count survives restart by scanning recorded plays.
- Yesterday's total uses the right date.
- Old checkpoint lines are ignored; new per-song checkpoints are not emitted.
- 1:30 duration equals 90 seconds; duration totals can exceed 24 hours.
- Daily report full-day total/remaining/next events and silence exclusions agree.
- Test SMTP and morning report only with authorized recipients.
- DNS/network/TLS failure logs an accurate failure, not false success.
- Startup/daily 30-day trim works without the Settings page visible.
- Invalid historical timestamps are handled deliberately.
- Log viewer newest-first limit/refresh and safe concurrent access.

### CLOCK and UI
- Clock shows the correct embedded face and live hour/minute hands.
- Correct 11:59/noon/midnight geometry and centered scaling.
- Clock open/close/reopen while real scheduled audio continues.
- No runtime-only construction of forms/controls.
- Every legacy theme/custom palette updates all pages correctly.
- Readable selected rows and single clean group borders/captions.
- Restored/maximized sizes and multiple DPI settings, without clipping/overlap.
- Correct taskbar/executable icon and actual version information.
- All menu/sidebar items lead to real functioning screens.
- Help opens locally and supports the agreed translation approach.
- X and File/Exit both release resources without AVs, hangs or orphan processes.

### Packaging
- Clean x86/x64 Debug and Release builds where required.
- Portable runtime test from a different drive letter/working directory.
- All native dependencies match architecture and are included.
- No credentials, private DB/logs/dumps, Songs or old_Songs in public packages.
- ZIP structure and launch scripts correct.
- Public download hashes match the tested release files.
- User acceptance of complete functionality before replacing production downloads.

## 23. Known Open Decisions and Risks

These are not excuses to omit work; resolve them before the relevant milestone.

- Production project/repository name and final executable name/version.
- Supported .NET target, minimum Windows version, deployment/runtime policy.
- Independent decoder/output choice and literal single-EXE expectations.
- Final license alignment: user says Apache 2.0; current LICENSE files say MIT.
- Exact late-event, sleep/resume and DST policies.
- Exact weekly-silence end-instant behavior and legacy/manual overlap handling.
- Whether to preserve old auto-enable-on-closing-administration behavior unchanged
  everywhere, or approve a refined policy. Current user-approved behavior is the
  baseline; do not reverse it by preference.
- Whether any optional legacy shutdown/restart function is actually implemented:
  miScheduleRestartClick contains a COMMENTED ScheduleShutdownAndRestart call.
  Do not advertise that as a working feature merely because a method exists.
- No actual MP3/WAV audio-engine migration was validated by the prototype.
- No live user data or email should be used in public CI.
- Do not mistake old root DBs or staging files for the currently supplied F: data.
- The old task workspace C:\New Delphi Projects\VCL2FMXConverter was absent during
  prototype work. Some tools failed with an invalid-working-directory error.
  Use the actual new project folder explicitly. Do not recreate unrelated project
  source or confuse that stale task folder with Carillon's source.

## 24. Suggested Opening for the New Session

"I have read FMX2CSharp.md and the current project instructions. I understand
that this is a full C# WPF conversion of Carillon 8.1, using SQLite and the approved
prototype design, including the analog clock and every existing feature. I will
keep the working Delphi application and F: database untouched, start with the
verified private copy, check Git, and prepare the required ZIP backup before
changing existing project files. I will first complete the feature map and verify
the audio/runtime choices, then implement and test the full application in a
separate production project."
Do not claim this handoff itself completes any migration milestone beyond the
design prototype and verified database copy. Keep this document and the parity
checklist updated as implementation decisions and validated results accumulate.

End of handoff.
