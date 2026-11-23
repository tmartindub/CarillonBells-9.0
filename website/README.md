# Westminster Chimes and Carillon Bells 9.0 Website

Open [the home page](v9/index.html) or [the downloads page](v9/downloads.html) in a browser to review the local preview. The `v9` folder contains the two static pages, their assets, and the four PDF guides. Keep those files together when previewing or staging the site.

## Production 8.1 inventory

The source checkout is `C:\New Delphi Projects\GitHub Pages`. The public site uses the two URLs below. Page layout and styling are embedded in the production HTML.

| Production file | Contents carried into the v9 preview |
| --- | --- |
| `index.html` at the [home URL](https://carillon-bells.github.io/) | Product overview, release status, features, software and guides, music and sound samples, hardware, privacy, and contact. The v9 page explains the C# and WPF rewrite and uses original product artwork and current app screenshots. |
| `downloads.html` at the [downloads URL](https://carillon-bells.github.io/downloads.html) | Software choices, guides, alphabetical music collections, hymnals, installation instructions, and contact. The v9 page explains x86 and x64, new installation, and upgrades. |
| `assets/carillon-icon.ico`, `assets/blue-domed-church.png`, `assets/main-screen.png` | Production branding, artwork and the 8.1 screen. The preview uses the approved v9 icon, Westminster artwork and screenshots exported from the v9 guides. |
| `docs/pdf/MIT_License.pdf` | License for the 8.1 source. The preview identifies the Apache 2.0 license for the v9 source through the v9 project. |
| `.nojekyll`, Google site verification HTML, `robots.txt`, `sitemap.xml` | GitHub Pages and search setup. These are host-level files. Keep site verification and `.nojekyll` in their existing location. Update crawl metadata and the sitemap when v9 is authorized for release. |

The production pages contain Google Analytics tags, Open Graph and Twitter card metadata, canonical URLs, and the public email contact. The downloads page lists two 8.1 software ZIPs with SHA-256 checksums, three v8.1 guides on GitHub, eight alphabetical music archives on Dropbox, and five hymnal or other music folders on Dropbox. The home page links to two Wix-hosted audio samples. The preview keeps the public contact and the existing music and sample URLs, with floating GTranslate on each page. Both preview pages use `noindex`. Analytics are reserved for an authorized production cutover. Version 8.1 is the current public software release, and the preview directs users there for software. The v9 guide links point to the local PDF copies.

### Existing page links

Keep the public page paths and section anchors so links saved from version 8.1 still land near the intended information when v9 replaces it.

| Public page and existing anchors | v9 content |
| --- | --- |
| Home `#top`, `#maintenance-update`, `#whats-new` | Home, release status and version 9 improvements |
| Home `#downloads`, `#music`, `#hardware` | Release choices, music and audio samples, and equipment guidance |
| Home `#privacy`, `#contact` | Privacy and contact |
| Downloads `#software`, `#guides`, `#instructions` | Software choices, PDF guides and installation steps |
| Downloads `#music-a-z`, `#hymnals`, `#contact` | Music collections, hymnals and contact |

## Release handoff

1. After release approval, use `C:\New Delphi Projects\CarillonBells v 9_0\export\32-bit` for Windows x86 and `C:\New Delphi Projects\CarillonBells v 9_0\export\64-bit` for Windows x64. Verify the New Install and Upgrade ZIPs and their matching Package Studio Pro Setup executables. Keep private databases, logs, credentials and local music outside the public site and release.
2. Publish the authorized v9 assets through the [v9 GitHub repository](https://github.com/tmartindub/CarillonBells-9.0). Label the upload links by architecture and package type, and publish the verified SHA-256 checksums. Change the development notices and software links to the confirmed release details. Keep an accessible route to the archived 8.1 release.
3. When replacing the live pages, put the two reviewed pages at their production URLs with the required relative assets and guides. Set canonical URLs, Open Graph and Twitter URLs and image metadata to the public destinations. Remove the draft `noindex` and update the sitemap dates after cutover. Keep the host's verification and GitHub Pages files. Review the analytics and privacy text together if analytics are enabled for the public version.
4. Verify both live pages on desktop and mobile. Check the old links listed above, the language selector, mail link, all guide PDFs, audio samples, and both architectures of the new-install and upgrade links.
