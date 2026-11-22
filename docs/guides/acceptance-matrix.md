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
| Delete, Refresh and navigation do not post into the wrong record. | Implemented; source review completed. Full scenario acceptance remains pending. |
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
| Portable runtime test from a different drive letter/working directory. | Implemented; source review completed. Full scenario acceptance remains pending. |
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

Authorized SMTP delivery, real hardware device removal, physical USB relocation/disconnect, multiple actual monitor DPI settings, and extended soak testing require their respective environment or user acceptance. Public production release and replacing 8.1 downloads remain unapproved.
