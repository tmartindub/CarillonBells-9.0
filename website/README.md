# Westminster Chimes and Carillon Bells 9.0 Website

Open [the home page](v9/index.html) or [the downloads page](v9/downloads.html) to review the site locally. The `v9` folder contains both pages, their assets, and the four PDF guides. Keep the files together for review and publication.

## Production 8.1 inventory

The source checkout is `C:\New Delphi Projects\GitHub Pages`. The public site uses the two URLs below. Page layout and styling are embedded in the production HTML.

| Production file | Corresponding v9 content |
| --- | --- |
| `index.html` at the [home URL](https://carillon-bells.github.io/) | Product overview, features, setup and upgrades, bell music, audio equipment, privacy, and contact. The v9 page uses the Westminster artwork and app screenshots to show what users can do. |
| `downloads.html` at the [downloads URL](https://carillon-bells.github.io/downloads.html) | Package choices, guides, music by title, hymnals, setup instructions, and contact. The v9 page explains how to pick the right Windows package and set up a flash drive. |
| `assets/carillon-icon.ico`, `assets/blue-domed-church.png`, `assets/main-screen.png` | The v9 site uses its approved icon, Westminster artwork, and screenshots exported from the v9 guides. |
| `docs/pdf/MIT_License.pdf` | License for the 8.1 source. The v9 site links to the Apache 2.0 license in the v9 project. |
| `.nojekyll`, Google site verification HTML, `robots.txt`, `sitemap.xml` | GitHub Pages and search setup. These are host-level files. Keep site verification and `.nojekyll` in their existing location. Update crawl metadata and the sitemap when v9 is authorized for release. |

The production pages contain Google Analytics tags, Open Graph and Twitter card metadata, canonical URLs, and the public email contact. The downloads page lists two 8.1 software ZIPs with SHA-256 checksums, three 8.1 guides on GitHub, eight music archives on Dropbox, and five hymnal or other music folders on Dropbox. The 8.1 home page links to two Wix-hosted recordings. The v9 site keeps the public email address and all thirteen music links, and provides four local PDFs and floating GTranslate. Both local pages use `noindex`.

### Existing page links

Both v9 pages carry the same Google Analytics measurement ID as the 8.1 site,
`G-GPWCTFRF87`. The home page's privacy section identifies this service. Retain
this ID when publishing v9 at the existing production URLs.

Keep the public page paths and section anchors so links saved from version 8.1 still land near the intended information when v9 replaces it.

| Public page and existing anchors | v9 content |
| --- | --- |
| Home `#top`, `#maintenance-update`, `#whats-new` | Home and version 9 improvements |
| Home `#downloads`, `#music`, `#hardware` | Setup and upgrades, music, and equipment guidance |
| Home `#privacy`, `#contact` | Privacy and contact |
| Downloads `#software`, `#guides`, `#instructions` | Software choices, PDF guides and installation steps |
| Downloads `#music-a-z`, `#hymnals`, `#contact` | Music collections, hymnals and contact |

## Audio files for release

The two MP3s are staged as untracked files in `C:\New Delphi Projects\GitHub Pages\assets\audio`. The 8.1 pages still use their existing sample links.

| File | Label |
| --- | --- |
| `assets/audio/westminster-chimes.mp3` | Westminster Chimes, Five O’Clock Strike |
| `assets/audio/amazing-grace.mp3` | Amazing Grace |

When publishing v9, add the two files to the website repository and place an audio player for each in the home page’s `#music` section. Use the relative paths above and check that both play from the published page.

## Publication configuration — November 26, 2026

Tommy authorized the v9 release and website publication. The home and downloads
pages are prepared for https://carillon-bells.github.io/ and downloads.html.
The production pages use Google Analytics G-GPWCTFRF87 and floating GTranslate.
The current canonical PDFs include Tommy's manually revised Quick Start guide.
The two approved MP3 samples are supplied under assets/audio.

The four public downloads are Installer ZIPs attached to the v9.0.0 release in
https://github.com/tmartindub/CarillonBells-9.0. The download cards provide SHA-256
checksums; source distributions/SHA256SUMS.txt records the same values. Portable
ZIPs under export/32-bit and export/64-bit remain installer build inputs and are
not the normal public download route.

Keep the original 8.1 HTML pages, assets and guide files under /v8.1/. Its existing
release assets at https://github.com/tmartindub/Carillon8_1/releases/tag/v8.1.0
remain unchanged and are linked directly from the v9 downloads page. Preserve
the host's Google verification file, .nojekyll and Git history.

Production robots.txt permits crawling and names sitemap.xml. The sitemap and
social metadata use the production URLs. Final publication results and live-link
verification belong in docs/publication-validation-20261126.json.

## User Guide refresh November 27 2026

The approved User Guide now includes feature introductions, smoother prose and
bold control names. The website PDF, standalone release PDF, and DOCX/PDF copies
inside every portable package and installer payload use the same canonical files.
Installer checksums were refreshed; the application binaries remain unchanged.
