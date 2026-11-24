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
