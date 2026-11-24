# Westminster Chimes and Carillon Bells — UI review

Last changed: November 24, 2026

## Open this build in a separate test copy

Leave the live D: program running. These review downloads do not install or
replace anything automatically.

1. Use a separate copy of your Carillon folder, including its database and
   music. Make a ZIP backup of that test copy before replacing its program.
2. Close the program running from the test copy.
3. Choose the 64-bit download for your existing 64-bit test setup, or the
   32-bit download for a 32-bit Windows installation. Extract the ZIP.
4. Put its **CarillonBells.exe** into the test folder containing the existing
   **databases** and music folders. Replace only that test program file.
5. Open **CarillonBells.exe** from the test folder. The download includes its
   own .NET runtime; a separate .NET installation is not needed.

Do not open two programs against the same database. This review does not
change version 8.1, the live D: executable, or your music collection.

## What to check

- **Email status:** successful email clears an older email error. Address
  failures name the relevant field. Test recipient remains optional for
  automatic morning reports. The morning report on November 24 had already
  succeeded; the displayed failure was older. No email was sent during testing.
- **Player:** after a song starts, the display reads **Last song played:**
  followed by the actual recording name. Moving the grid cursor does not
  change it. Before the first play it says **No songs played this session**.
- **Groups:** both date columns now have calendar buttons. Test choosing a
  date, saving it, and cancelling another edit. Merely opening a calendar must
  not create an edit.
- **Random music:** enter dates as **10/1** and **10/31**. No year is shown or
  accepted. These ranges already repeat annually. Cross-year ranges still work.
- **Clock hands:** Original is preserved. Eighteen black alternatives replace
  the previous choices. Click a smaller card to use the existing large preview.
  Save applies the choice; Cancel changes keeps the previous pair.
- **Search:** the × clears the search and returns the full list.
- **Log:** schedule messages identify maintenance reasons. Long log entries
  wrap so the full message can be read. The missed-occurrence policy is unchanged.
- **Sidebar:** the button section is lower, with more space around Today's
  schedule. Smaller windows retain a vertical navigation scrollbar.

## Try a font safely

Open **System → Font settings…**. Choose a font and size, then click **Check
every page**. Your current appearance stays active during checking.

When all checks pass, **Apply checked font** becomes available. If any value or
label would be cut off, the choice is refused and the result names the problem.
Choose a smaller size or a narrower font and check again. Changing the candidate
requires another check. **Restore defaults** returns to the original appearance.

The size range is 14–20; 18 is the original size. The usable maximum can be lower
for your data. After widening Purpose and separating the organization/date
lines, **Original 18, Arial 14, Arial 18 and Tahoma 16 passed** with the test data.
Each installation must check its own values and display.

Save or cancel other edits before checking fonts. Checks include the current
field values, minimum window sizes, and current display density. Saved choices
are checked again at startup and when the main window changes display scaling.
Different data or another computer can change which choices pass. Read-only
reports and lists retain vertical scrolling; input-field overflow does not count
as fitting.

The clock artwork and the size/layout of its right-hand preview are preserved.
The four guides, HTML Help and proposed website pages were refreshed on
November 24 to describe the current behavior, with new demonstration screenshots.

## Rollback checkpoints

The pre-change ZIP and numbered checkpoint ZIPs are on
**H:\CarillonBells v 9_0**. Each numbered ZIP identifies the change and Git commit.
They contain source/documentation snapshots that can be restored and rebuilt.
The final release backup additionally includes the validated review downloads.
See **docs/UI-improvements-checkpoints.md** for what each point contains.
