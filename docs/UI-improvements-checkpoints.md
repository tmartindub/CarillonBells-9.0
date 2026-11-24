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
