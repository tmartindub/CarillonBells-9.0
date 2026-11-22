from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]
handoff=(root/'docs/guides/FMX2CSharp-handoff.md').read_text(encoding='utf-8-sig')
section=handoff.split('## 22. Acceptance Matrix - Completion Gate')[1].split('## 23.')[0]
groups=[];current=None
for line in section.splitlines():
    if line.startswith('### '):
        current=(line[4:],[]);groups.append(current)
    elif line.startswith('- '):current[1].append(line[2:])
    elif line.startswith('  ') and current and current[1]:current[1][-1]+=' '+line.strip()
automated={
 'Existing F: database copy':'Baseline hash verified; separate private development copy opened in UI.',
 'Composite playlist':'Composite-key update and duplicate constraints pass repository tests.',
 'Both older wide':'Wide migration and seven-slot resolver pass isolated database tests.',
 'Unknown/backup':'Unknown table and pre-existing wide backup preservation pass migration tests.',
 'Save System first':'System / Email / System owned-field sequence passes database tests.',
 'NULL time':'NULL versus midnight passes schedule and SQLite round-trip tests.',
 'Duplicate slots':'Duplicate time suppression and distinct simultaneous songs pass schedule tests.',
 'Repeat limits':'Domain validation enforces 1–5; real WAV repeat completion passes audio tests.',
 'Full-day report':'Report builds independent snapshots; schedule/count source regression cases pass.',
 'Report schedule':'Independent full-day schedule builder tested without live queue mutation.',
 'Sleep/resume':'Injected forward/backward clock discontinuity tests pass; real sleep remains an environmental check.',
 'January 2':'Injected-clock January 2 and January 3 runtime maintenance checks pass.',
 'Same-day, overnight':'Same-day, overnight starting-weekday and equal-time cases pass domain tests.',
 'Exact start/end':'Inclusive compile end and exclusive live end pass domain tests.',
 'Legacy Ash':'Overlapping legacy reasons, exact Ash Wednesday end and overnight funeral interval pass runtime tests.',
 'Multiple simultaneous':'Multiple legacy reason calculation passes runtime tests; endpoint restoration requires hardware acceptance.',
 'Real MP3/WAV':'Real reference MP3 and generated WAV decode/output, pause/resume/stop, repeats and completion pass.',
 'Repeated load':'Twenty repeated full decode/duration/load cycles for MP3 and WAV pass; Windows MPEG decoder absent from tested process.',
 'Current-day count':'Played Song count, disabled logging and ignored checkpoints pass isolated log tests.',
 "Yesterday's total":"Date-specific log counting is covered by repository/log regression and report source review.",
 'Old checkpoint':'Checkpoint messages do not affect recorded play counts; no new checkpoint emitter.',
 '1:30 duration':'m:ss duration and totals over 24 hours pass deterministic tests.',
 'Invalid historical':'Invalid historical timestamp retention and 30-day trim pass log tests.',
 'Clock open/close':'Real scheduled file plays while analog clock is visible; close/reopen passes audio/runtime tests.',
 'Correct 11:59':'Hand geometry is tested; original artwork and centered clock inspected visually.',
 'No runtime-only':'All controls/templates are declared in XAML; source review completed.',
 'Public download':'No public C# release assets published; requires separate release approval.',
 'User acceptance':'Pending user evaluation; Delphi 8.1 remains live and unchanged.'
}
environment=['Test SMTP','DNS/network/TLS','Audio callbacks','System power','Same-folder second','Restored/maximized sizes and multiple DPI','Missing/bad files','Silence start/end log','Startup or edit during','Log viewer','Help opens']
automated.update({
 'Windows tray':'External endpoint notifications and application-to-Windows volume changes pass on x86/x64; actual tray UI was not automated.',
 'Manual mute before/during':'Preexisting manual mute and a manual mute selected during silence are retained; original endpoint state restored after tests.',
 'All native dependencies':'Clean self-contained x86/x64 packages pass SQLite and real audio/runtime tests with native dependencies included.',
 'No duplicate dispatch':'Persistent ledger, schedule re-enable and injected forward/backward discontinuity tests pass.',
 'X and File/Exit':'Shared close path reviewed; X was exercised without hangs or remaining application windows.',
 'Save/Cancel preserve':'UI Save persisted a disposable repeat edit and Cancel restored its saved value; stable-key/group state logic reviewed.',
 'Clock shows':'Original embedded clock artwork and analog hands inspected in the running UI.',
})
lines=['# Carillon Bells 9.0 acceptance record','','Last changed November 21, 2026.','',
'This is an experimental separate application. Implementation is present across the handoff domains. Verification below records only checks actually performed; it does not declare production acceptance or authorize replacement of Delphi 8.1.','',
'Evidence files: architecture-specific `domain-database-results-win-*.txt`, `audio-runtime-results-win-*.txt`, `build-results.txt`, and the source/schema inventories in this folder.','',
'Manual UI checks covered the private development copy opening 120 songs, computed schedule, original analog clock, administration forms, Save/Cancel on disposable data, persisted palette selection, dark contrast, restored and maximized windows, and orderly close. No real email was sent.','']
for heading,items in groups:
    lines+=['## '+heading,'','| Handoff requirement | Verification |','|---|---|']
    for item in items:
        evidence=next((v for k,v in automated.items() if item.startswith(k)),None)
        if evidence is None:evidence='Implemented; source review completed. Full scenario acceptance remains pending.'
        if any(item.startswith(k) for k in environment):evidence='Implemented; targeted test or physical/environmental acceptance remains pending. Do not treat as verified.'
        lines.append('| '+item.replace('|','/')+' | '+evidence+' |')
    lines.append('')
lines+=['## Decisions and deliberate changes','',
'- Separate Carillon Bells 9.0 project and GitHub repository; protected 8.1 and prototype remain unchanged.',
'- New code Apache 2.0; prior MIT notices retained.',
'- .NET 10 LTS, self-contained x86 and x64 deployment; no single-EXE claim.',
'- NLayer handles MP3 decoding and duration; Windows waveOut receives PCM through NAudio.',
'- Missed events after detected discontinuities are skipped. A persistent local occurrence ledger prevents duplicate dispatch.',
'- The source compile/live weekly-silence end distinction is preserved. Equal start/end is inactive.',
'- Legacy mute toggles are represented by active intervals and combined reasons, allowing correct startup during silence.',
'- A date change builds the new day immediately, with separate 00:05 daily maintenance.',
'- Evaluation mode suppresses automatic email. No real SMTP delivery was attempted.',
'- Optional legacy shutdown/restart was commented out in the source and is not advertised as an implemented feature.','',
'## Remaining production gates','',
'Authorized SMTP delivery, real hardware device removal, physical USB relocation/disconnect, multiple actual monitor DPI settings, and extended soak testing require their respective environment or user acceptance. Public production release and replacing 8.1 downloads remain unapproved.','']
(root/'docs/guides/acceptance-matrix.md').write_text('\n'.join(lines),encoding='utf-8')
print('Recorded',sum(len(items) for _,items in groups),'handoff acceptance items without claiming unperformed checks.')
