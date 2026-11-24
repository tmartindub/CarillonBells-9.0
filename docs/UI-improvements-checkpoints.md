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
