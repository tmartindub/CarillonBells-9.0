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
