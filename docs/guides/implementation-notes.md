# Carillon Bells 9.0 implementation continuation

Last changed November 21, 2026.

The original handoff is preserved in `FMX2CSharp-handoff.md`. The separate application is implemented in this repository; evaluation and acceptance evidence is recorded in `acceptance-matrix.md`. This does not promote it to the live production release.

The user approved Apache 2.0 for new code with prior MIT notices retained, skipping missed events after discontinuities, and creating a separate GitHub repository. Delphi 8.1 source, its repository/releases and website are protected and unchanged.

The current workspace `CarillonBells v 9_0` is the production-development destination. The source prototype and Delphi directories were only read. Before creating project files, a ZIP of the handoff and prototype reference files was saved at `H:\CarillonBells v 9_0\CarillonBells_9_0_pre-change_20261121_200619.zip`. The workspace was empty before work began.

The application targets .NET 10 LTS, using a project-local 10.0.404 SDK and self-contained 10.0.15 x86/x64 runtimes. Native deployment contains multiple files. NLayer explicitly decodes MP3 for both playback and duration; NAudio WinMM outputs PCM and WASAPI supplies endpoint management. No Windows MPEG decoder was loaded during repeated real-file tests. MailKit uses asynchronous explicit STARTTLS.

Weekly silence retains the source inclusive schedule-compilation end and exclusive live end; equal start/end is inactive. Legacy silence uses combined active reasons, avoiding blind toggles. A persistent occurrence ledger enforces at-most-once dispatch across re-enable/restart. A new date rebuilds the day's queue immediately; separate daily maintenance remains after 00:05. Annual settings use the source January day >= 2 guard, and groups use January 3 exactly.

The verified private baseline remains unchanged. Development copies are under ignored build output. Public packages contain no working database, music, logs or credentials. Database import opens its source read-only, validates a temporary copy, preserves old/unknown tables, performs transactional upgrades and time normalization, backs up the local target as ZIP and can export a conversion report.

All windows, palette parameters, styles and templates are in XAML. The original clock and splash artwork were extracted without modifying the source forms. HTML help includes local search, glossary, index, print styling and GTranslate support. Both guides have DOCX/PDF companions and editable TOC fields, rendered and visually inspected with LibreOffice and Poppler.

Both x86 and x64 self-contained packages passed 41 domain/database and 100 audio/runtime/endpoint checks. The extracted x64 ZIP repeated all 141 checks successfully on USB drive H: with a different working directory. Package-manifest hashes and private-data exclusions passed for both ZIPs. The package source commit is recorded in `source distributions/release-manifest.json`; subsequent commits add validation evidence without changing application behavior.
