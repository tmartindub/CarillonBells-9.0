# Carillon Bells 9.0 acceptance record

Last changed November 21, 2026.

This is an experimental separate application. Implementation is present across the handoff domains. Verification below records only checks actually performed; it does not declare production acceptance or authorize replacement of Delphi 8.1.

Evidence files: architecture-specific `domain-database-results-win-*.txt`, `audio-runtime-results-win-*.txt`, `build-results.txt`, and the source/schema inventories in this folder.

Manual UI checks covered the private development copy opening 120 songs, computed schedule, original analog clock, administration forms, Save/Cancel on disposable data, persisted palette selection, dark contrast, restored and maximized windows, and orderly close. No real email was sent.

## Data and editing

| Handoff requirement | Verification |
|---|---|
| Existing F: database copy opens; original/baseline hashes stay unchanged. | Baseline hash verified; separate private development copy opened in UI. |
| Startup never switches to a different empty sample database. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Composite playlist keys identify the correct song. | Composite-key update and duplicate constraints pass repository tests. |
| Add single/multiple MP3/WAV files; duplicate detection and duration work. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Edit all twelve play times, dates, weekdays, group and repeats. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Save/Cancel preserve the selected song, group state and scroll position. | UI Save persisted a disposable repeat edit and Cancel restored its saved value; stable-key/group state logic reviewed. |
| Delete, automatic reload and navigation do not post into the wrong record. | Stable-key Save/Cancel and changed-group selection checks pass; manual playlist Refresh was removed by user instruction. Full add/delete scenario acceptance remains pending. |
| Pending edits are handled when closing/navigating. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Save System first, then Email, then System again: no stale-update conflict. | System / Email / System owned-field sequence passes database tests. |
| Both older wide and current row-based rotation databases are handled safely. | Wide migration and seven-slot resolver pass isolated database tests. |
| Unknown/backup tables and user data survive migrations. | Unknown table and pre-existing wide backup preservation pass migration tests. |

## Schedule/calendar

| Handoff requirement | Verification |
|---|---|
| All weekdays, inclusive date ranges, midnight and 23:59:59. | Implemented; source review completed. Full scenario acceptance remains pending. |
| NULL time is absent; 00:00:00 is a real event. | NULL versus midnight passes schedule and SQLite round-trip tests. |
| Duplicate slots do not duplicate the same event; simultaneous distinct songs remain distinct and deterministic. | Duplicate time suppression and distinct simultaneous songs pass schedule tests. |
| Repeat limits match 1..5, not the prototype's 1..12. | Domain validation enforces 1–5; real WAV repeat completion passes audio tests. |
| Enable/disable and all administrator-screen pause/resume semantics. | Implemented; source review completed. Full scenario acceptance remains pending. |
| No duplicate dispatch or overlap during repeated enable/disable. | Persistent ledger, schedule re-enable and injected forward/backward discontinuity tests pass. |
| Events around midnight and daily 00:05 rebuild. | Implemented; source review completed. Full scenario acceptance remains pending. |
| January 2 settings and January 3 group maintenance. | Injected-clock January 2 and January 3 runtime maintenance checks pass. |
| Leap year, cross-year seasonal windows and next holiday calculations. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Sleep/resume, clock changes and DST have documented tested behavior. | Injected forward/backward clock discontinuity tests pass; real sleep remains an environmental check. |
| Report schedule remains correct when live queue is paused. | Independent full-day schedule builder tested without live queue mutation. |

## Silence

| Handoff requirement | Verification |
|---|---|
| Twelve slots, Purpose and all weekday flags save/reload. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Same-day, overnight, overlap and adjacent windows. | Same-day, overnight starting-weekday and equal-time cases pass domain tests. |
| Exact start/end boundary behavior, including compile/live difference. | Inclusive compile end and exclusive live end pass domain tests. |
| Startup or edit during an active window. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Silence start/end log entries and correct restoration. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Manual mute before/during a silence window is respected. | Preexisting manual mute and a manual mute selected during silence are retained; original endpoint state restored after tests. |
| Legacy Ash Wednesday/Holy Week/All Souls/funeral dates/times work. | Overlapping legacy reasons, exact Ash Wednesday end and overnight funeral interval pass runtime tests. |
| Multiple simultaneous silence reasons do not unmute each other. | Multiple legacy reason calculation passes runtime tests; endpoint restoration requires hardware acceptance. |
| Past occurrences disappear from Remaining Schedule without deleting rules. | Implemented; source review completed. Full scenario acceptance remains pending. |

## Playback and Windows integration

| Handoff requirement | Verification |
|---|---|
| Real MP3/WAV play, duration, pause/resume, stop, repeats and next event. | Real reference MP3 and generated WAV decode/output, pause/resume/stop, repeats and completion pass. |
| Repeated load/unload/duration cycles without the old decoder failure. | Twenty repeated full decode/duration/load cycles for MP3 and WAV pass; Windows MPEG decoder absent from tested process. |
| Missing/bad files and disconnected audio device fail gracefully. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Manual and scheduled playback coordinate safely. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Windows tray volume/mute changes update app controls and vice versa. | External endpoint notifications and application-to-Windows volume changes pass on x86/x64; actual tray UI was not automated. |
| Audio callbacks do not hang the UI during close or device changes. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| System power hold and release work. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Same-folder second-instance prevention and lock release work. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |

## Reporting/logging

| Handoff requirement | Verification |
|---|---|
| Current-day count survives restart by scanning recorded plays. | Played Song count, disabled logging and ignored checkpoints pass isolated log tests. |
| Yesterday's total uses the right date. | Date-specific log counting is covered by repository/log regression and report source review. |
| Old checkpoint lines are ignored; new per-song checkpoints are not emitted. | Checkpoint messages do not affect recorded play counts; no new checkpoint emitter. |
| 1:30 duration equals 90 seconds; duration totals can exceed 24 hours. | m:ss duration and totals over 24 hours pass deterministic tests. |
| Daily report full-day total/remaining/next events and silence exclusions agree. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Test SMTP and morning report only with authorized recipients. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| DNS/network/TLS failure logs an accurate failure, not false success. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Startup/daily 30-day trim works without the Settings page visible. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Invalid historical timestamps are handled deliberately. | Invalid historical timestamp retention and 30-day trim pass log tests. |
| Log viewer newest-first limit/refresh and safe concurrent access. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |

## CLOCK and UI

| Handoff requirement | Verification |
|---|---|
| Clock shows the correct embedded face and live hour/minute hands. | Original embedded clock artwork and analog hands inspected in the running UI. |
| Correct 11:59/noon/midnight geometry and centered scaling. | Hand geometry is tested; original artwork and centered clock inspected visually. |
| Clock open/close/reopen while real scheduled audio continues. | Real scheduled file plays while analog clock is visible; close/reopen passes audio/runtime tests. |
| No runtime-only construction of forms/controls. | All controls/templates are declared in XAML; source review completed. |
| Every legacy theme/custom palette updates all pages correctly. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Readable selected rows and single clean group borders/captions. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Restored/maximized sizes and multiple DPI settings, without clipping/overlap. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| Correct taskbar/executable icon and actual version information. | Implemented; source review completed. Full scenario acceptance remains pending. |
| All menu/sidebar items lead to real functioning screens. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Help opens locally and supports the agreed translation approach. | Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified. |
| X and File/Exit both release resources without AVs, hangs or orphan processes. | Shared close path reviewed; X was exercised without hangs or remaining application windows. |

## Packaging

| Handoff requirement | Verification |
|---|---|
| Clean x86/x64 Debug and Release builds where required. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Portable runtime test from a different drive letter/working directory. | The extracted self-contained x64 ZIP passed all 41 domain/database and 100 audio/runtime/endpoint checks on USB H: while launched with a different working directory. |
| All native dependencies match architecture and are included. | Clean self-contained x86/x64 packages pass SQLite and real audio/runtime tests with native dependencies included. |
| No credentials, private DB/logs/dumps, Songs or old_Songs in public packages. | Implemented; source review completed. Full scenario acceptance remains pending. |
| ZIP structure and launch scripts correct. | Implemented; source review completed. Full scenario acceptance remains pending. |
| Public download hashes match the tested release files. | No public C# release assets published; requires separate release approval. |
| User acceptance of complete functionality before replacing production downloads. | Pending user evaluation; Delphi 8.1 remains live and unchanged. |

## Decisions and deliberate changes

- Separate Carillon Bells 9.0 project and GitHub repository; protected 8.1 and prototype remain unchanged.
- New code Apache 2.0; prior MIT notices retained.
- .NET 10 LTS, self-contained x86 and x64 deployment; no single-EXE claim.
- NLayer handles MP3 decoding and duration; Windows waveOut receives PCM through NAudio.
- Missed events after detected discontinuities are skipped. A persistent local occurrence ledger prevents duplicate dispatch.
- The source compile/live weekly-silence end distinction is preserved. Equal start/end is inactive.
- Legacy mute toggles are represented by active intervals and combined reasons, allowing correct startup during silence.
- A date change builds the new day immediately, with separate 00:05 daily maintenance.
- Evaluation mode suppresses automatic email. No real SMTP delivery was attempted.
- Optional legacy shutdown/restart was commented out in the source and is not advertised as an implemented feature.

## Remaining production gates

Authorized SMTP delivery, real hardware device removal/storage disconnect, multiple actual monitor DPI settings, and extended soak testing require their respective environment or user acceptance. USB relocation on H: and a different working directory passed. Public production release and replacing 8.1 downloads remain unapproved.

## Import display repair — November 21, 2026

Reproduced the reported failure in an isolated x64 portable copy: create a blank database, import the Downloads reference database, and close Administration. The library contained 120 songs but showed one group and blank song cells until restart. Accessibility exposed the records even while their visual layout was incomplete.

`MainWindow.LoadSongs` now replaces the library with a complete observable collection snapshot instead of clearing and adding individual records to the live grouped view. It reapplies the current search and restores expansion and scroll state at dispatcher Loaded priority, after binding and layout. All controls remain defined in XAML; no runtime UI construction or database schema changes were added.

Manual x64 checks passed: imported song names and lengths display immediately without restart; collapse and expand; collapsed groups remain collapsed after Refresh; search results remain filtered after Refresh; selecting another filtered song updates its schedule; Cancel restores an edited repeat count and resumes scheduling; Save preserves selection, search, and visible song rows; reopening displays the imported library. The isolated database was the only database edited during these checks. The original Downloads database SHA-256 remained unchanged.

Release development build and self-contained x64/x86 publishes succeeded. Existing 41 domain/database regression checks passed for each architecture. This repair did not repeat the earlier audio, SMTP, hardware, or soak checks. The pre-change tracked-source ZIP is `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-import-refresh-fix_20261121.zip`.

## Automatic playlist reload and schedule restoration — November 21, 2026

Removed the playlist Refresh button and handler. Save, adding songs, deletion, and returning from data administration reload automatically. Reload preserves the composite song key, search, group expansion and scroll position. A saved group change selects the edited key; deleting the selected record selects a neighboring record. Selection restoration preserves the XAML binding, and the grid does not synchronize selection to the collection view's current position.

Playlist editing restores the schedule state captured when the edit session began. Administration now restores its prior state instead of always enabling scheduling. Add and Delete pause scheduling during mutation and restore the prior state afterward. Administration performs one reload on return. A failed save retains the edit session; Cancel discards the clone without a needless playlist reload.

Release build and both self-contained publishes passed. All 55 regression checks passed per architecture, including enabled/paused Save and Cancel, stable selection, changed-group identity, and retained grid selection binding after dispatcher layout. Manual x64 checks confirmed the Refresh button is absent, a nonfirst song remains selected after Administration, initially enabled scheduling resumes on close, and initially paused scheduling stays paused. Add/delete completion and import were source-reviewed for the same restoration paths; those dialogs were not repeated manually in this change. No SMTP, audio, hardware or soak checks were repeated.

Help and both editable/PDF guides were updated; rendered guide pages were reviewed. The pre-change tracked-source ZIP is `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-automatic-refresh_20261121.zip`. Delphi 8.1 and its releases remain untouched.

## Manual playback and missing music — November 21, 2026

Added a XAML row double-click event that selects the clicked song and invokes the same playback action as Play, matching the reference FMX behavior. Left double-clicks on song rows play the selected song; group headers and column headers do not invoke this handler. Existing repeat settings remain in effect. Missing audio now reports the resolved full path and explains that importing a database does not copy music.

The reported desktop test folder lacked the referenced Music and Songs files. With the user's confirmation that audio was on USB, the test binaries received a ZIP backup and the USB source was read only. Copied and hash-verified 129 Music files and 87 additional missing relative playlist files into the desktop test folder. A read-only audit then found zero missing non-random playlist audio paths while USB was connected. Absolute external paths remain unchanged and still require their source drive. No database path rewriting or private audio distribution was introduced.

Release development and self-contained x64/x86 builds passed. All 56 regression checks passed per architecture, including the new missing-file diagnostic. Manual x64 validation used two silent WAV records: double-clicking the second row played the second song, Stop released playback, and Play started the selected song again. The actual reported USB MP3 passed all 100 x64 audio/runtime/endpoint checks. No subjective listening, real SMTP or long-running soak acceptance is claimed. Guides/help were updated and rendered pages reviewed. Pre-change source ZIP: `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-manual-playback_20261121.zip`; desktop binary ZIP: `H:\CarillonBells v 9_0\CarillonBells_9_0_desktop_pre-playback_20261121.zip`.

## U.S. dates with slashes — November 21, 2026

User-facing numeric dates now use U.S. month/day/year with slash separators. Windows controls declare en-US in XAML; the process uses en-US culture without regional user overrides. Native calendar fields use the standard U.S. short display (for example, 1/2/2027); administration text fields and grids use MM/dd/yyyy (01/02/2027). Date headers and guidance, the main date banner, silence detail, database analysis/import timestamps, log viewer, new log entries, and report timestamps follow U.S. slash order. Existing parsed log timestamps are reformatted in report sections without rewriting historical logs.

Administration formats only its editable view snapshots and accepts that display baseline as unchanged. Group, random-directory and system date edits still serialize as ISO yyyy-MM-dd inside SQLite. Optional blanks remain blank; recalculated administration dates use U.S. display. No database migration or date reinterpretation was introduced.

Release development and both self-contained publishes passed; all 61 regression checks passed per architecture. Added checks cover ambiguous 01/02 input being January 2, ISO persistence, clean view snapshots, nullable settings, old/new log parsing and report dates. Manual x64 inspection confirmed native calendar fields, System dates, group dates and labels use U.S. slash ordering. Guides/help were updated and rendered pages reviewed. Audio, SMTP and soak tests were not repeated for this presentation change. Pre-change source ZIP: `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-US-dates_20261121.zip`; desktop binary ZIP: `H:\CarillonBells v 9_0\CarillonBells_9_0_desktop_pre-US-dates_20261121.zip`.

## Song status wording and editor spacing — November 21, 2026

Changed the next-event card to NEXT SONG and its empty enabled-queue message to No remaining songs. Renamed the view property/binding consistently. Added a 14-unit XAML spacer between Group and Times to play, aligned their labels, and increased clearance above the play-time fields. No scheduling or email behavior changed. Release development and both portable publishes passed; all 61 regression checks passed on each architecture. Manual x64 inspection confirmed the empty-queue wording, separated boxes, and all twelve play-time fields without clipping.

For overnight testing, launch CarillonBells.exe directly without --evaluation and confirm the Portable footer. Leave scheduling/logging enabled and the computer awake across midnight through 5:00 a.m. The runtime rebuilds at the date boundary, performs daily schedule maintenance at/after 12:05 a.m., trims logs at/after 12:10 a.m., and attempts the morning email at/after 5:00 a.m. Automatic email is suppressed in evaluation mode. The test database's email-enabled flag, SMTP host/port/login presence, morning-recipient presence and logging flag were checked read-only; no credentials were printed and no email was sent by the agent. A successful manual Test morning report should precede the overnight run. Preview report saves a local HTML file; the automatic morning path generates and emails HTML without saving a standalone report file. Actual overnight operation and SMTP delivery remain user acceptance tests.

Pre-change source ZIP: `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-song-label-spacing_20261121.zip`.

## D: portable installation and relocation — November 22, 2026

Authorized deployment target: D: volume ICCPORTBL 3, identified by Carillon_9_0_test.txt. Existing 8.1 installation and music folders were preserved. The initialized v9 folder is D:\CarillonBells_9_0; copy the whole folder to move it. The normal application is bin\Release\CarillonBells.exe in this workspace. Both now contain the self-contained win-x64 runtime.

Pre-change ZIP backups completed and their archive directories were opened successfully before edits:
- H:\CarillonBells v 9_0\CarillonBells_9_0_pre-D-portable_workspace_20261122_091958.zip (9,790 entries, 1,525,029,188 bytes).
- H:\CarillonBells v 9_0\CarillonBells_9_0_pre-D-portable_USB_20261122_091958.zip (4,806 entries, 3,877,976,868 bytes).

All manual and scheduled playback, including random music, resolves inside the running EXE folder. Legacy drive letters are remapped to that folder layout. Relative paths escaping the installation, network paths and drive-relative paths are rejected. New file/folder selections are stored relative to the installation and must be inside it. The existing v9 configuration was retained and its single absolute playlist path normalized. Required audio was copied, without deleting existing music: 2,014 files, 1,389,992,195 bytes, hash-verified in the application and USB copies. Both databases pass integrity_check; all 120 playlist paths are relative and resolve (random placeholders excluded from physical file checks), and all six configured random directories contain MP3 files. The legacy random-directory setting was also copied locally.

Release and self-contained win-x64/win-x86 builds passed. Each passed 67 domain/database/WPF checks and 100 audio/runtime/endpoint checks. The six added path cases cover relative audio, legacy drive relocation, rooted audio and rejection of traversal, drive-relative and network paths. The final self-contained bin\Release copy passed all 67 checks again. Direct D: execution passed all 67 domain and 100 audio checks using D:-local music. A complete copy of D:\CarillonBells_9_0 to a disposable folder on C: passed the same 67 and 100 checks and a complete read-only database/audio-path audit. These runs used C:\Windows as the working directory and an intentionally invalid DOTNET_ROOT, demonstrating EXE-relative data and bundled runtime use. Audio suites cover real MP3 decoding/playback, WAV repeat completion, scheduled playback with the clock open, dispatch discontinuities, endpoint notifications and mute/volume restoration. Raw sanitized USB and relocation results are saved beside this matrix.

Manual application-folder and USB UI inspection confirmed 120 songs, the next-song card and populated schedule. Manual playback logs confirmed each installation played from its own Songs folder. Both validation instances were closed without editing playlist settings. Automated and manual GUI checks used evaluation mode; no email was sent. Real SMTP delivery, extended overnight operation, physical device-removal and testing on a second physical computer remain separate environmental checks. This remains experimental software.

The build script's -Publish -Runtime win-x64 -PortableRoot 'D:\CarillonBells_9_0' workflow publishes first, then copies and hash-verifies application files into bin\Release before updating the initialized USB copy. It skips databases, logs, music, locks and test-result files. It never silently overwrites portable user data. Local help and the guide-generation source describe the new path behavior; existing DOCX/PDF guides were not regenerated in this deployment. Source/private-data audit passed. Only the public CarillonBells-9.0 remote exists for v9 in the configured remote and connected account inventory; no separate private v9 repository was found.
