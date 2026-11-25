# UI improvement checkpoints — November 24, 2026

The live D: installation and Delphi 8.1 are not deployment targets for this work.
Each numbered checkpoint has a Git commit and a matching ZIP on
`H:\CarillonBells v 9_0`. ZIP names identify the checkpoint and commit.
They contain the tracked project source and documentation needed to rebuild that point.
The initial ZIP is `CarillonBells_before_UI_improvements_20261124_104059.zip`.

## 01 — Email status and address validation

Read-only inspection of D:\logs\CarillonPlayLog.txt confirmed an older email
failure on November 23, followed by a successful morning report on November 24
at 05:00. Successful email now clears only a previous email error, preserving
unrelated errors. Missing or invalid recipients identify the relevant field.
A blank test recipient does not affect morning reports. SMTP login names no
longer have to be email addresses when a valid From address is configured.
No messages were sent during validation. Automatic retry policy is unchanged.

## 02 — Player wording

The player shows `Last song played: [actual recording]` after playback begins,
including the actual randomly selected recording. Grid selection no longer
changes this text. Before the first play it reads `No songs played this session`.
The redundant lower text line was removed. Playback controls are unchanged.

## 03 — Group calendar pickers

Both group date columns now contain native WPF calendar pickers defined in XAML.
Typed dates, blank dates, pending changes, Save/Cancel, U.S. slash display, and
ISO database storage are retained. Group range validation still runs on Save.

## 04 — Annual random-music dates

Random-music dates display as `10/1`, without a year or forced leading zeros.
Input validation accepts real month/day values only, including February 29,
and rejects a typed year. The internal reference year for newly entered values
is 2000 so leap day remains representable. Existing ISO stored dates remain
compatible; matching still ignores the year and includes cross-year ranges.

## 05 — Black clock-hand catalogue

Original is unchanged. Eighteen new black vector pairs replace all fourteen
previous alternatives: Taper, Needle, Sword, Lancet, Spade, Open spade, Diamond,
Open diamond, Leaf, Open leaf, Breguet, Serpentine, Twin scroll, Lyre, Fleur,
Gothic, Vienna, and Nouveau. Their tips stop inside the minute track and their
decoration remains near the center. Cards use five columns and smaller thumbnails.
The right preview and full clock face/layout are unchanged. Prior numbered
preferences select the replacement at that number; Original remains ID zero.

## 06 — Clear song search

An accessible × button appears inside the search field when text is present.
It clears the filter, preserves the selected song when present, and returns
keyboard focus to the search field. It does not change scheduling.

## 07 — Schedule log reasons

Schedule state transitions identify startup, manual action, song maintenance,
adding/deleting songs, Groups, random music, silence windows, email/reports, or
database maintenance. Changing maintenance tabs identifies the current reason.
Closing maintenance records restoration to the previous state; a previously
disabled schedule is not enabled. No change was made to discontinuity policy.

## 08 — Sidebar spacing

The button section begins approximately three centimeters lower at standard
Windows scaling, with additional space around Today's schedule. All spacing
is editable in MainWindow.xaml. The existing vertical scrollbar keeps lower
buttons accessible at smaller window sizes.

## 09 — Validated application fonts

System → Font settings opens a Fonts tab. The choices are Original,
Bahnschrift, Segoe UI, Arial, Tahoma, Verdana, and Calibri, with base sizes
14–20. All original font values remain editable resources in App.xaml.
Candidate resources are applied only to separate copies of the XAML screens.
Validation covers every song, all administration rows/tabs, today's schedule,
masked and visible passwords, native date calendars, the latest 1,000 log
entries, the clock menu, and startup text. The checker completes deferred WPF
layout before measuring the full text, including text that would be clipped or
ellipsized. Input-field overflow is rejected rather than accepted as scrolling.
Read-only reports/log lists retain vertical paging; long song titles and log
entries wrap. The clock artwork and preview dimensions remain unchanged.

The checker uses current data, minimum supported window sizes, the current
work area, and the active window's display density. Apply rechecks before
saving data\fonts.json. Saved choices are checked at startup and on main-window
DPI changes; invalid choices restore Original with a notice. Restore defaults
is always available. Pending data edits must be saved or cancelled first.
No candidate changes the active appearance before validation succeeds.

Visual review with Original and Arial covered the playlist, schedule overlay,
Groups/calendar popup, System, Email/reports, Random music, Silence windows,
Themes, Fonts, Clock hands, Database, Log, and the full clock. Review also fixed
a malformed Themes arrow, cramped button padding, and narrow weekday columns.
Group calendars use typed nullable date values so simply viewing them cannot
create pending changes. Tests confirm previewing leaves database bytes,
scheduling, and active font resources unchanged, rejects overflow/oversize,
persists valid preferences, and safely handles invalid saved preferences.

A read-only SQLite snapshot of the live D: data was tested privately and deleted
afterward. Arial 14 passed all checks. Original 18 and Arial 18 were rejected for
longer silence-purpose text at the minimum window size; Arial 18 also exceeded
the organization/date line. This confirms the limits respond to actual data.
Repeated grid layouts were replaced by checks of every distinct editor value
plus the longest wrapped title, avoiding quadratic work on large song lists.

## 10 — Black clock center refinement

The new black hub covers the brown spindle already present in the background
artwork. Its radius matches Original, while the new shafts/tips stay slender.
The background image, clock proportions, and preview layout are unchanged.

## 11 — Help and review instructions

The existing local HTML Help now describes the new player wording, search ×,
group calendars, annual month/day dates, black clock hands, email status, log
reasons, and font workflow. Translation widgets and protected product names are
preserved. UI-improvements-review.md gives safe installation and acceptance
steps for a separate test copy. Existing PDF/DOCX guides are not rewritten.

## 12 — Validated review downloads

November 24, 2026. Executables were built from source commit **7bae2b2**.
Both self-contained native Windows executables completed **131 passing checks,
zero failures**. This includes scheduler, repository, player, annual dates,
clock rendering, font bounds/persistence/fallback, and preview isolation checks.
Both packages ran without a separate system .NET 10 installation. Builds had
zero warnings and errors. The final 64-bit executable was also opened in an
isolated fictitious-data copy for visual review; that copy was then closed.
The live D: process was left running. No email was sent during validation.
This validation does not substitute for an overnight operational test.

The separate downloads are in **export/UI-improvements-20261124**. Each ZIP
contains only CarillonBells.exe and review instructions: no database, music,
credentials, or personal font preferences. ZIP contents, executable hashes,
and PE architecture were verified. Per-architecture test evidence is in its
validation subfolder. Existing export/32-bit and export/64-bit downloads were
not replaced, and no website/release deployment was performed.

SHA-256 of the executable, including the identical copy inside each ZIP:

- win-x64: `6A43BFB44F9101ABF02BD4D3C2AE492AE5BB3EBF860FFDC4D6DE31F827A41B41`
- win-x86: `375E2870BEB1CA6E8F77A6DB6449E0F9059E2106663F02D7C05B11ADB77F71F1`

The source/document privacy audit passed after classifying two unchanged
website mailto links as existing public contact information. No other private
reference values or database/audio/dump artifacts were found in publishable
source/documents. The strict audit script itself remains unchanged.

Checkpoint 12 additionally includes both validated review ZIPs and the test
evidence, so this checkpoint can supply the runnable review executables as
well as a source snapshot. Earlier numbered ZIPs contain source/documentation
snapshots for rebuilding their respective intermediate changes.

## 13 — Authorized D: installation

On November 24, 2026 the user explicitly authorized replacing
**D:\CarillonBells.exe** with the validated 64-bit review build. The application
was already closed. Before replacement, its executable, databases folder, and
data/settings folder were archived to the private backup disk:
**H:\CarillonBells v 9_0\D_before_UI_review_20261124_121504.zip**.
The archived executable hash was verified against the prior D: executable.

Only D:\CarillonBells.exe was replaced. Its SHA-256 matches the win-x64 value
in checkpoint 12. The application was reopened and observed loading 120 songs,
with Schedule enabled, 51 occurrences today and 28 queued, and the next song
at 12:30 PM. No manual playback or test email was triggered.
Music folders and the separate Carillon.exe were not replaced.
An after-change executable ZIP is saved as
**H:\CarillonBells v 9_0\D_after_UI_review_20261124.zip**.

## 14 — Silence purpose entry limit

November 24, 2026. The user approved limiting Purpose to the editor's available
width with a character of spare room. The existing editable XAML column now
connects its editor to width validation in AdministrationWindow. Text is measured
with the actual font, size and display density; an M-width reserve is left.
Oversized additions and multiline pastes are refused as a whole, keeping the
prior accepted text and displaying a short explanation. No fixed character
count or silent truncation is used. Existing long values are preserved and can
be shortened incrementally. The all-page font checker remains unchanged;
existing long descriptions must be shortened before a larger font can pass.

Both self-contained Windows executables passed **152 checks**, zero failures.
New checks cover variable-width letters, the entry boundary, oversized and
multiline replacements, clearing, preservation/shortening of old long values,
and Arial/Tahoma/Verdana at sizes 14, 16, 18 and 20. An isolated fictitious-data
editor test also accepted Night service, rejected an oversized replacement,
and saved only the accepted value, verified by reading back the test database.

Review packages and results are in **export/Purpose-limit-20261124**. The D:
application was already closed; its executable/databases/settings were backed
up in **H:\CarillonBells v 9_0\D_before_Purpose_limit_20261124.zip** before the
validated 64-bit executable was installed. Music and Carillon.exe were untouched.
After reopening, D: displayed all 120 songs with scheduling enabled, 51 events
today and 28 queued. No manual playback or test email was triggered.

Executable SHA-256:

- win-x64: `FCC0ABEBB6E3757983EC798205D95C260F2F6033CB32A454FB546D3F96066DBE`
- win-x86: `5E78D035ED24844269924E7F60AB3B4F1310B78BAF9B738806D5D0D0ACD84851`

The pre-change source ZIP is **Before_purpose_entry_limit_20261124.zip** on H:.
The numbered after-change checkpoint includes source plus runnable review
packages and validation evidence for rollback.

## 15 — Silence column space correction

November 24, 2026. Start/End columns were reduced from 130 to 105 each,
returning 50 pixels to Purpose. Extra editor padding was removed for these
three fields. No descriptions were shortened. Diagnostics verified that the
font checker measures the actual constrained cell content: at the minimum
window, Purpose is 229 wide with 200 usable for displayed text. The longest
unchanged live description requires 175.5 at Tahoma 16, 198.3 at Original 18,
and 195.1 at Arial 18. A private read-only-source snapshot passed every page
with Tahoma 16 and Original 18. Arial 18 now passes Silence; only the main
organization/date line still fails. The temporary snapshot was deleted.

Both native self-contained builds passed 154 checks, zero failures, including
new minimum-window column allocation and descriptive Purpose coverage. This
checkpoint precedes the separately authorized organization/date header change;
it was not installed on D: independently. The after-change ZIP includes both
validated intermediate executables, source, and evidence for rollback.

## 16 — Separate organization and date/time header

November 24, 2026. The user additionally approved giving Organization a wrapping
line and moving date/time beneath it. Both are separate editable XAML controls;
the header row grows automatically instead of clipping to a fixed height.
TodayLabel now contains only the existing USA date/time format. The font-check
messages distinguish Organization name from Date/time line. No font limits or
overflow checks were bypassed.

With the same unchanged private snapshot of D: data, all four tested profiles
passed every page: Original 18, Arial 14, Arial 18, and Tahoma 16. Both final
native self-contained Windows executables passed **155 checks**, zero failures.
Builds completed with zero warnings/errors. The snapshot was deleted after use.
The final packages/results are in **export/Header-and-silence-fit-20261124**.

The user authorized closing D:. Its administration and main windows closed
cleanly without unsaved-edit prompts. The pre-install ZIP is
**H:\CarillonBells v 9_0\D_before_Header_silence_fit_20261124.zip**, containing
the prior executable, databases and settings. Only D:\CarillonBells.exe was
replaced; database bytes were verified unchanged during replacement. The app
was reopened, and the separate header lines and compact time columns were
visually reviewed. All 120 songs loaded; scheduling was enabled after closing
the unchanged review window. Existing descriptions/font preferences were
preserved. No manual playback or test email was triggered.

Executable SHA-256:

- win-x64: `0C53E8A3A77DC735251C6C604C3C2304A80FCFE5DED8BF76F2F80774699F0288`
- win-x86: `045FCC381FBD3F89B99EFD6CA522E9680ACE000E122DE845C89F97B168243134`

Checkpoint 15 is the pre-header source/runnable snapshot. The numbered
checkpoint 16 after-change ZIP contains the final source, both review packages,
and validation evidence. The Delphi 8.1 executable/music were not replaced.

## 17 — Visible font application confirmation

November 24, 2026. After Apply checked font successfully validates and saves,
the application now shows a Font applied information dialog naming the family
and size and explaining that no Save click is needed. The same confirmation
remains in the Fonts report, with an applied-and-saved status beside Save.
Failure paths still report Not applied and do not show a success dialog.

Both native Windows builds passed 155 regression checks, zero failures. The
actual Check every page / Apply workflow was exercised in a separate
fictitious-data copy: the dialog appeared and the persistent status remained
after dismissal. This did not change live font preferences. The disposable
test copy was closed and removed after validation.

D: was closed cleanly without an unsaved-edit prompt, backed up to
**H:\CarillonBells v 9_0\D_before_Font_feedback_20261124.zip** (executable,
database and settings), and restarted with the validated 64-bit executable.
Startup was visually confirmed: 120 songs loaded and Schedule enabled.
No manual playback or test email was triggered. Existing data/preferences,
music and Carillon.exe were preserved.

Executable SHA-256:

- win-x64: `735C2CE276B6040E5C925BBE2F2BD314D561A6C6ABE5DBC8690A05DADD11E545`
- win-x86: `BBFA776F01B334CC98CE16CC9C4DF311746193F5CDDF585FD6344445038868C0`

The pre-change source ZIP is **Before_font_apply_feedback_20261124.zip** on H:.
The after-change checkpoint contains the source, both executables and results
from **export/Font-feedback-20261124**.

## 18 — Move the player details away from System Volume

November 24, 2026. The flexible spacer between playback buttons and the
PLAYER/Last song played display could grow to 208 pixels. It is now a fixed
24 pixels, moving the details up to 184 pixels left and giving the title more
room. The existing 20-pixel gap before the System Volume boundary remains.
Long titles still wrap, and the player content is contained within its own
grid. These are editable XAML layout parameters; no runtime positioning code
was added. Song names, data, font preferences and volume behavior are unchanged.

Both self-contained native Windows builds passed **167 checks**, zero failures.
Twelve new layout checks use the reported Ash Grove filename and a very long
unbroken title across Arial 14, Tahoma 16 and Arial 18, at narrow and wide
window sizes. They verify the fixed gap after the buttons and the title's
right edge remaining clear of System Volume. Builds had zero warnings/errors.

D: was closed cleanly, backed up to
**H:\CarillonBells v 9_0\D_before_Player_spacing_20261124.zip** (executable,
database and settings), and restarted with the verified 64-bit executable.
Only the executable was replaced. The pre-change source ZIP is
**Before_player_spacing_20261124.zip** on H:. The after-change checkpoint
contains source, both executables and test results from
**export/Player-spacing-20261124**.
The restarted D: layout was visually confirmed: player details start directly
after the fixed button gap, System Volume is separate, all 120 songs loaded,
and scheduling remains enabled. No manual playback or test email was triggered.

## 19 — Shift player details right by approximately 2.5 cm

November 24, 2026. At the user's request, the fixed gap after the playback
buttons was increased from 24 to 118.5 Windows layout units. This moves the
PLAYER/Last song played display right by 94.5 units (approximately 2.5 cm at
standard Windows scaling). Wrapping and the clear boundary before System
Volume remain. No other layout, font preference, or song data was changed.

Both native Windows builds passed 167 checks, zero failures. The existing
long-title checks were updated to verify the revised fixed position and
separation from volume controls across fonts and narrow/wide window sizes.
The pre-change source ZIP is **Before_player_position_adjustment_20261124.zip**
on H:. D: was closed cleanly and backed up (executable/database/settings) to
**H:\CarillonBells v 9_0\D_before_Player_position_adjustment_20261124.zip**.
Only D:\CarillonBells.exe was replaced and restarted. The after-change
checkpoint contains source, both executables, and results from
**export/Player-position-adjustment-20261124**.

Executable SHA-256:

- win-x64: `FED56473AFF5BA6624E88B3715275C0BE707225BE1E1C687FC74CE387CEFCDE9`
- win-x86: `64C184D1DF9A0E11AD40C002013188E16D3C687BC2ADCDCC5B06D3D45F6F173D`

## 20 — Hide and synchronize the legacy Ash Wednesday end

November 24, 2026. The legacy end editor remains defined in editable XAML but
is collapsed in v9. The visible Ash Wednesday date is authoritative. Manual
System saves and manual/background recalculation synchronize both fields.
Repository writes also enforce the pair, including clearing both dates and
refusing a conflicting legacy-end-only update. No column was removed.

The existing compatibility check detects stale/missing legacy end values,
makes its normal database ZIP backup, and copies the visible/start date to the
end in a transaction. Reopening an already synchronized database does not
repeat the repair. Ash Wednesday silence still uses one day and its existing
time boundaries; Holy Week/All Souls calculations were not changed.

Both self-contained native Windows executables passed **178 checks**, zero
failures. Eleven added checks cover individual/conflicting date writes, blank
dates, stale database repair and repeated checks, hidden UI, pending manual
recalculation, manual save, January annual maintenance, and single-day silence.
Builds completed with zero warnings/errors.

D: was closed cleanly and backed up (executable/database/settings) to
**H:\CarillonBells v 9_0\D_before_Ash_Wednesday_sync_20261124.zip** before
replacement/restart. Read-only verification against this backup confirmed that
both live fields are **2027-02-10** and every other System setting is unchanged.
The temporary verification database was removed. The System page was visually
checked: only Ash Wednesday 02/10/2027 is shown. All 120 songs loaded and the
schedule was enabled after closing the unchanged review window. No manual
playback or test email was triggered. Music and Carillon.exe were untouched.

The pre-change source ZIP is **Before_Ash_Wednesday_sync_20261124.zip** on H:.
The numbered after-change ZIP contains source, both executables and results
from **export/Ash-Wednesday-sync-20261124**.

Executable SHA-256:

- win-x64: `DA3F0A7010732B1F4D30EEEF61D320259576E80E8515CBC7E858A4273FB2C7A3`
- win-x86: `EB90276F51F529097B5AFE140445F29F21271BC241202DD4D0105496587A2DE8`

## 21 — Calendar buttons on hover or keyboard focus

November 24, 2026. The shared editable XAML DatePicker template hides its
calendar button at rest while reserving the button's space. It appears when
the date field is hovered or has keyboard focus and remains visible while the
calendar is open. Standard DatePicker template parts retain typed dates,
native calendar selection, date bindings, and popup behavior. A compact calendar
symbol is drawn in XAML. This applies to playlist and group date pickers;
there is no runtime positioning/hover code or new helper class.

Both native Windows builds passed **184 checks**, zero failures. Six additional
checks verify hidden-but-reserved button space, stable editor width, and
unchanged date values at Arial 14, Tahoma 16 and Arial 18. The actual UI was
tested with fictitious data: hidden buttons at rest, hover and keyboard focus
independently visible, opening the calendar, selecting September 10, 2027,
saving and verifying that value, and hiding the buttons after leaving both
fields. The isolated test app was closed and its disposable data removed.

The pre-change source ZIP is **Before_calendar_hover_20261124.zip** on H:.
The after-change checkpoint includes both executables and test evidence from
**export/Calendar-hover-20261124**. At this source checkpoint, D: installation
was pending: the running administration window reported unsaved changes.
No pending edits were saved or discarded without the user's decision.

Executable SHA-256:

- win-x64: `F9444F1EC8116C2A1104B587113E64F35F4E3390C70E0CCC2751594E705A18F9`
- win-x86: `B5CE64E2FFDE1836616E6DB481B58B9267B6B9D8466D7627A208A46367050961`

## 22 — Calendar-hover installation on D:

November 24, 2026. The user stopped D: and authorized continuation after the
pending-edits pause. Its executable, database and settings were backed up to
**H:\CarillonBells v 9_0\D_before_Calendar_hover_20261124.zip**; the executable
inside the ZIP was hash-verified against the prior installed file.

The validated 64-bit executable from checkpoint 21 was copied to
**D:\CarillonBells.exe**, verified against its recorded SHA-256, and restarted.
Visual startup review confirmed all 120 songs loaded, scheduling enabled, and
calendar buttons hidden at rest. No live dates or font settings were edited,
and no manual playback/test email was triggered. Music and Carillon.exe were
preserved. The after-install checkpoint includes source and the same tested
executables/results from **export/Calendar-hover-20261124**.

## 23 — Groups prompts only for actual changes

November 24, 2026. The deep dive reproduced the fault before changing logic:
rendering Groups and running the real close-time CommitEditors marked a row
Modified even though all Current values equalled Original values. The old
SettingsDirty test used GetChanges, so that row flag produced a false prompt.
The before-fix failing test evidence is retained in the validation folder.

The fix changes only the Groups portion of SettingsDirty. Added/deleted rows
still need saving; Modified rows need saving only if at least one column value
differs from Original. It does not clear row state indiscriminately, call
AcceptChanges over genuine edits, remove date editor commits, or suppress the
prompt itself. Existing handling of other administration tables/passwords and
clock-hand choices is unchanged. No helper class or runtime UI was added.

Twenty new regression checks exercise actual binding/layout and close-time
commits; untouched close and unchanged database bytes; changing/reverting group
name, From, Until and play time; adding/removing a new group; deleting/undoing
an existing group; bound calendar changes/reversion; genuine Save; clean state
after Save; and Cancel restoring clean values without writing the edit.
Both native self-contained builds passed **204 checks**, zero failures.

A temporary read-only-source snapshot of actual D: data reproduced **16 rows
marked Modified with zero differing values**. The fixed version correctly
reported clean. Original 18, Arial 14, Arial 18 and Tahoma 16 also passed the
existing all-page checks. The private snapshot was removed after testing.
The GUI was separately tested with fictitious data: untouched Groups closed
without a prompt, while a real group-name edit prompted and saved correctly
(confirmed by reading back the fictitious database).

The pre-change source ZIP is **Before_Groups_dirty_fix_20261124.zip** on H:.
D: was closed cleanly, backed up (executable/database/settings) to
**H:\CarillonBells v 9_0\D_before_Groups_dirty_fix_20261124.zip**, and restarted
with the verified 64-bit executable. Review artifacts and before/after evidence
are in **export/Groups-save-prompt-20261124**. The after-change ZIP includes
source, both runnable executables, and validation evidence.

Executable SHA-256:

- win-x64: `5E8178E0D19F37AC11D10A2EE4A7ED79C1A84FF5165CCBCC5297443A29B27F2B`
- win-x86: `E38E047EF373216204F9525817D1177378178291FC702D9C594369D9AFA588BA`

After restart, untouched Groups was opened and closed **twice on D:** without
a save prompt. The group-table digest matched its pre-install value afterward;
no live group records changed. All 120 songs loaded, and scheduling returned
to enabled after both maintenance windows closed. No manual live playback or
test email was triggered. The fictitious test app and private snapshots were
removed; rollback ZIPs and required evidence were preserved.

Executable SHA-256:

- win-x64: `6295A126D1FD4F2E724E5ADB9E91164D5D6D8F85DD9D1D04CCBF4C0B48572EF8`
- win-x86: `612E19C814763F6029ED068A7C4CCE117A8AF6BC11BA5D7052EC6A82734F10FC`

## 24. Documentation and proposed website refresh — November 24

Updated the User, Engineering, Database Migration and Quick Start manuscripts,
DOCX guides and companion PDFs, searchable HTML Help, README and proposed v9
home/download pages. These cover the font check/apply confirmation, annual
month/day random dates, automatic playlist updates and search clearing, actual
last-played recording, hidden synchronized Ash Wednesday compatibility date,
calendar hover/focus behavior, genuine Groups change detection, maintenance
reasons, email status/recipients, header/player spacing and eighteen black clock
hand alternatives. The illustrated migration steps and recovery procedures remain.

Seven current app screenshots were taken from a disposable Example Church
installation with synthetic settings and silent WAV files. D: was left running.
No application code, executable, live database, release package or production
website was changed. The current D: binary's SHA-256 is
`5E8178E0D19F37AC11D10A2EE4A7ED79C1A84FF5165CCBCC5297443A29B27F2B`.

LibreOffice updated all four editable tables of contents and exported the PDFs.
The packaged DOCX renderer produced page images; every page was visually checked:
User 13, Engineering 13, Migration 19 and Quick Start 4 (49 total). A sparse Quick
Start spillover page was corrected, and the font workflow now has an illustration.
All 120 local HTML references, manuscript figure paths, populated TOC fields,
PDF page counts, site/guide PDF hashes and preserved translation/noindex settings
passed checks. The existing shared stylesheet retains the one-column mobile
breakpoint. Browser preview of the local pages was blocked by browser URL policy;
desktop/mobile browser appearance and live translation were not runtime-tested.

The private-data audit's two matches were the unchanged pre-existing public
contact mailto lines in the proposed pages. Comparison with HEAD confirmed no new
private credential or recipient disclosure. New screenshots contain demonstration
data only. The Help builder now uses the maintained User manuscript instead of
an obsolete import, keeping future Help updates aligned with the guide.

Pre-change ZIP: `H:\CarillonBells v 9_0\Before_docs_web_refresh_20261124.zip`.
The requested completion checkpoint ZIP uses prefix `Checkpoint24_DocsWeb_`
and the resulting Git commit. PDF validation hashes are recorded in
`guides/documentation-refresh-validation.json`. The refreshed Help is available
as local HTML source; embedding it into an executable requires the next approved
application build. Existing release packages must be rebuilt before publication.

## 25. Clear guides and refreshed local distribution November 24

Pre-change ZIPs on H: were verified before replacement:
`Before_docs_packages_cleanup_20261124_source.zip` (164 entries) and
`Before_docs_packages_cleanup_20261124_packages.zip` (14 entries).

Rewrote Quick Start as a five-page first-installation guide with a separate cover
and contents, readable body text, corrected page boundaries and one schedule
illustration. Removed upgrade/rollback executable references from Quick Start.
Replaced the User Guide's competing setup sequence with opening an installed
copy and keeping its folders together. Migration now distinguishes the current
program, optional Migration Assistant and older recovery program explicitly.
All four guides, Help, README and the proposed setup page explain AM/PM entry
and conversion to 24-hour display, including noon and midnight. Keep-alive text
describes both Windows sleep prevention and periodic drive writes accurately.

Rebuilt the x86/x64 app and Migration Assistant with current Help. Each app passed
204 self-tests. Rebuilt four USB ZIPs and four Package Studio Setup programs;
the auditor verifies ZIP CRCs, PE architectures, executable and guide hashes,
current Help and exact embedded ZIP payloads. Reusable Setup projects are under
`tools/package-projects`. The migration package lock acquired the SDK's existing
10.0.15 ILLink build-task entry during single-file publishing; product dependency
versions and application C# source were not changed.

All 50 guide pages were visually reviewed; all 120 local HTML references passed.
The starter database passes quick_check, contains 14 schedule records and no
email credentials/recipients; its existing 129 supplied recordings were retained.
Raw native test results are preserved in `export/release-validation-20261124`.
The initially open Quick Start PDF was closed by Tommy and its canonical file
replaced. The local PDF now matches the reviewed website and distribution copies.

Tommy requested no push/publication and no changes to the two scheduling items
raised in review. D: remains running with its previous executable. Overnight
acceptance remains Tommy's test. The source, documentation and release manifest
are committed locally; the final source ZIP is regenerated from the resulting
commit. A completion ZIP on H: uses prefix `Checkpoint25_Final_` followed by
that commit. No GitHub, production website or version 8.1 files were changed.

## 26. Website first Quick Start walkthrough November 24

Replaced the Quick Start manuscript with a continuous new-installation walkthrough:
visit the named website, distinguish New Install from Upgrade, identify Windows
with concrete System type examples, find the downloaded ZIP, extract it onto the
USB, double-click CarillonBells, test sound and verify one scheduled song.
Mouse actions, keyboard selection, file extensions, drive letters, dates and
AM/PM conversion are explained at their first use. Existing installations are
directed to the Migration Guide. Two current application illustrations accompany
the steps. The guide cautions readers to wait if the website still offers 8.1.

The eight-page DOCX and PDF were regenerated with LibreOffice, the contents
updated and every rendered page visually checked. Canonical, proposed website
and D: documentation copies match. Four local USB ZIPs and their matching Setup
programs are refreshed with the same guide pair and audited before committing.
Application binaries and the running D: app are unchanged; native tests were not
rerun for this documentation-only change. Website download activation remains
part of the separately authorized publication step.

Pre-change ZIP: `Before_QuickStart_walkthrough_20261124_195940.zip` on H:.
The previously verified Checkpoint25 packages ZIP protects the unchanged release
inputs. Source changes and the refreshed manifest are committed locally only.
No push or publication is authorized for this work.

## 27. Direct USB Setup and selected destination verification November 25

Pre-change source and release ZIPs on H: are named
`CarillonBells v 9_0/Before_20261125_direct_USB_install.zip` and
`Package Studio Pro/Before_20261125_direct_USB_install.zip`. Both passed CRC
verification. D: guides were backed up as
`D_Docs_before_direct_USB_20261125_verified.zip` before replacement.

Package Studio local commit `7219e4c` makes every Setup action use the currently
selected folder, permits non-Windows local drive roots, and preserves protection
for Windows folders, the Windows drive root and network-share roots. Verify
checks files supplied by the selected package, avoiding false failures for user
data retained from an earlier New Install when applying Upgrade. Original Verify
logic reproduced the changed-destination failure; the corrected actual FMX form
handlers passed 33 assertions. The repeatable runner passed and removed its
private SUBST mapping and fixture. Its source remains in the Package Studio repo.

All four Carillon Setup downloads now embed individual application files instead
of a ZIP to unpack. New Install supplies the starter database and music. Upgrade
supplies only the main program, Migration Assistant and guides. One shared
installation identity permits Upgrade over an earlier New Install. The matching
ZIPs remain alternative downloads. `tools/build_usb_setups.py` rebuilds the Setup
files from those reviewed ZIPs; `tools/audit_packages.py` compares every embedded
payload file with its ZIP counterpart and records both source commit identities.

Actual embedded x86/x64 runtimes passed 22 operations: root and space-containing
subfolder installs, Verify, Upgrade over New Install and legacy fixtures,
deliberate damage rejection, Repair and disposable removal. All installed New
Install files matched their embedded sources. A modified valid SQLite database,
user recording, supplied music and legacy executable survived Upgrade unchanged.
No physical USB or Windows 10 run is claimed; Tommy should retest there using the
rebuilt installer, replacing his earlier desktop copy. No Carillon app was
started by these package tests and no real email was sent.

Quick Start, Migration and Engineering guide DOCX/PDF pairs were updated with
direct Setup instructions, explicit Install-then-Verify steps, destination examples
and separate ZIP alternatives. Their 41 pages were reviewed, including full-size
inspection of the changed installation pages. D: and proposed-site guide copies
match the reviewed files. All 44 local proposed-site references pass. The main
x86/x64 app binaries and live D: executable retain their prior SHA256 values.

The older general Package Studio acceptance script was not counted as passed:
its machine-folder sample requests elevation and could not start its runtime in
this session. The focused actual-form and final embedded-runtime tests above
passed. Raw results are retained in `export/release-validation-20261125`.
Application self-tests were not repeated for unchanged application binaries.
Both projects are committed locally and source ZIP checkpoints saved on H:.
No push, publication, live D: restart or version 8.1 modification was performed.

## 28. Installer ZIP downloads and explicit package roles November 25

Pre-change source and all twelve release files were saved in the verified ZIP
`H:/CarillonBells v 9_0/Before_Installer_ZIP_Downloads_20261125.zip` before
replacement or removal. Public downloads now live under export/downloads in
32-bit and 64-bit subfolders. Each of the four names ends Package_Setup.zip and
contains one self-contained Setup EXE, the relevant PDF guide and START_HERE.txt.
Users extract that package on the computer, then run Setup to select the USB.

The original portable ZIPs remain in export/32-bit and export/64-bit because the
Setup builder needs them and they provide an advanced manual-install option.
They are not required alongside Setup on a user's USB. Operator labels distinguish
those inputs from public downloads. The four redundant loose Setup EXEs were
removed only after validation and confirmation that they matched their ZIP backup.
The app and Migration Assistant EXEs remain required build inputs.

The updated auditor validates the outer download CRC, exact three-file layout,
guide and instruction bytes, installer hash and all individual embedded payload
files against the portable input ZIP. The wrapped installers passed 22 actual
embedded-runtime operations, including drive-root/subfolder installs, Upgrade
preservation, Verify, damage rejection and Repair. No native installer or Carillon
application logic was changed. Existing actual-form regression evidence remains
applicable; it was not rerun for this packaging-only change.

Quick Start, Migration and Engineering now distinguish the Installer ZIP from the
manual portable ZIP, explain extraction onto the computer and direct installation
onto the USB, and accurately describe unsigned-download warnings. The 41 guide
pages retain their counts. Comparison with the pre-change PDFs identified eight
changed pages, all visually checked at full size; remaining page text is unchanged
from the prior reviewed edition. All 44 local proposed-site references pass.

ZIP wrapping does not digitally sign software or eliminate Windows warnings.
Microsoft's SmartScreen reputation and Attachment Manager guidance were checked;
no security settings, file-origin marks or certificate configuration were changed.
The x86/x64 app and live D: executable remain unchanged. Physical USB/Windows 10
acceptance and Internet-download reputation are not claimed by these local tests.

The reader-open D: Quick Start PDF was initially deferred. After Tommy closed
it on November 25, the D: PDF was replaced and its SHA256 verified. All six
affected D: guide files and all proposed-site PDFs now match their canonical
copies; `docs/installer-zip-validation-20261125.json` records no pending copies.
Raw evidence is under export/release-validation-installer-zip-20261125. Changes
and the manifest are committed locally with a source ZIP checkpoint on H:.
No push or publication is authorized or performed.


## 30. Shared USB-root migration and clear folder selection — November 25

Tommy reported that both v9 executables were beside the old installation in E:\.
The destination guard rejected E:\V9 because it was inside the assistant folder.
The narrow correction permits a new direct child when source and v9 template
share a drive root. The drive root itself, existing folders, reserved data folders,
nested source destinations and children of a non-root application folder remain
protected. Migration stages its copy and leaves the old installation unchanged.

Source is detected beside the assistant or chosen with Browse database. Destination
now has an existing parent, an unused new folder name and a read-only full path.
A real native file/folder-picker walkthrough completed a 7.2.4 root-to-V9 migration
on disposable T:. Original database, executable and music hashes were unchanged.
The template copy selects v9 files and declared dependencies, includes versioned
Documentation, and excludes unrelated old executables/DLLs and loose legacy Help.

Final development tests: 226 PASS; final self-contained x64 and x86 tests: 224 PASS
each, all exit 0. Cases cover 7.2.4 conversion, 8.1 preservation, backup, copy hashes,
existing/reserved/nested destination rejection, changed-source rejection and
cancellation. Temporary screenshot capture in an earlier test run caused a shutdown
crash after assertions passed; that capture instrumentation was removed before
final testing. Physical Windows 10 / E: acceptance remains Tommy's test.

Migration and Engineering guides were regenerated with updated contents, paired
PDFs and new source/destination screenshots. Changed migration pages 17–19 and
engineering material were visually checked; full-page overviews were inspected.
The affected guide pairs on D: and proposed-site PDFs match canonical copies.
All 44 local site references pass. The live D: executable and 8.1 remain untouched.

The four portable packages and four public Installer ZIPs are rebuilt and audited
against the current binaries and guides. Installer runtime checks cover 22 operations
including root/subfolder installation, preservation, Verify failure and Repair.
Detailed evidence is in docs/migration-root-validation-20261125.json and
export/release-validation-migration-20261125. A pre-change ZIP and numbered Git
checkpoint are retained on H:. Local commits only; no push or publication.
