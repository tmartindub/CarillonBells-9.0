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

## Release handoff

The November 24 documentation refresh updates the local proposed pages, their
four guide PDFs and screenshots for fonts, annual random dates, calendar buttons,
the player and eighteen black clock hand alternatives. Demonstration screenshots
use Example Church. Production pages and version 8.1 remain unchanged.
The local 32-bit and 64-bit New Install and Upgrade ZIPs and their matching
Package Setup programs have now been rebuilt with the current executable, Help
and revised guides. The package manifest records their hashes. These are local
release candidates; no publication or URL switch has been authorized. Keep the
proposed pages marked noindex/nofollow until publication is approved.

1. After release approval, use `C:\New Delphi Projects\CarillonBells v 9_0\export\32-bit` for Windows x86 and `C:\New Delphi Projects\CarillonBells v 9_0\export\64-bit` for Windows x64. Verify the New Install and Upgrade ZIPs and their matching Package Studio Pro Setup executables. Keep private databases, logs, credentials and local music outside the public site and release.
2. Publish the authorized v9 assets through the [v9 GitHub repository](https://github.com/tmartindub/CarillonBells-9.0). Add the confirmed download links and verified SHA-256 checksums to the New Install and Upgrade cards under `#software`. Label links by Windows version and package type. Keep an accessible route to the archived 8.1 release.
3. Put the reviewed pages at their production URLs with the required relative assets, audio files, and guides. Set canonical URLs, Open Graph and Twitter URLs and image metadata to the public destinations. Remove the local `noindex` and update `robots.txt` and the sitemap for public indexing. Keep the host's verification and GitHub Pages files. Review the analytics and privacy text together if analytics are enabled.
4. Verify both live pages on desktop and mobile. Check the old links listed above, the language selector, mail link, all guide PDFs, audio samples, and both architectures of the new-install and upgrade links.
