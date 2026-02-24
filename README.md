# Exicon - SVG Icon Fetcher

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Unity 6+](https://img.shields.io/badge/Unity-6000.0%2B-black.svg)](https://unity.com)

A Unity Editor tool to browse, search, and download SVG icons from popular open-source icon libraries — directly into your project.

> **No icons are bundled.** Icons are downloaded on-demand from their original open-source repositories. Users are responsible for respecting each library's license.

## Features

- **Browse 11 icon packs** — Lucide, Tabler, Heroicons, Bootstrap Icons, Feather, Ionicons, Phosphor, Octicons, Iconoir, and more
- **Custom URL download** — paste any GitHub SVG link to download a single icon
- **Folder import** — paste a GitHub folder URL to batch-download all SVGs inside it
- **PNG export** — optionally convert SVGs to PNG sprites at configurable sizes (32×32 to 1024×1024)
- **Color override** — recolor all icons on download to a single custom color
- **Select All / individual selection** — pick exactly the icons you need
- **Parallel downloads** — icons download in batches of 10 for speed
- **Cancellation** — stop a download at any time; already-fetched icons are kept
- **Automatic LICENSE fetching** — downloads the repository's license alongside icons
- **SVG sanitization** — cleans SVGs for Unity compatibility (`currentColor` → `#000000`, strips unsupported attributes)
- **1-hour cache** — minimizes GitHub API usage (60 req/hour unauthenticated limit)
- **Auto-organization** — icons are saved in subfolders by pack name
- **Non-intrusive UI** — status messages appear inline in the tool window (no popup dialogs)

## Requirements

| Requirement | Version |
|---|---|
| Unity | 6.0 or later |
| SVG support | Unity 6.3+ (built-in) or [`com.unity.vectorgraphics`](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@latest) for earlier versions |

## Installation

### Option 1: Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** → **Add package from git URL...**
3. Paste:
   ```
   https://github.com/experir/exicon-svg-icon-library-fetcher.git
   ```
4. Click **Add**

### Option 2: Clone into your project

```bash
git clone https://github.com/experir/exicon-svg-icon-library-fetcher.git Assets/SvgIconFetcher
```

## Quick Start

1. In Unity, go to **Tools > SVG Icon Fetcher**
2. Choose an icon pack from the dropdown, or paste a GitHub URL
3. Click **Load Icons** to fetch the list
4. *(Optional)* Enable **Export as PNG** and choose a size, or set a **Color Override**
5. Select the icons you want and click **Download Selected**

Icons are saved to `Assets/Icons/<PackName>/` by default.

### PNG Export (optional)

Requires **Unity Vector Graphics**:
- **Unity 6.3+** — built-in, no action needed
- **Earlier versions** — install [`com.unity.vectorgraphics`](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@latest) via Package Manager

When enabled, a `.png` sprite is saved alongside each `.svg`. PNGs are imported as **Sprite (Single)** with **Alpha Is Transparency** enabled.

## Supported Icon Packs

| Pack | License | Website |
|---|---|---|
| [Lucide](https://lucide.dev) | ISC | lucide.dev |
| [Tabler Icons](https://tabler.io/icons) (Outline & Filled) | MIT | tabler.io/icons |
| [Heroicons](https://heroicons.com) (Outline & Solid) | MIT | heroicons.com |
| [Bootstrap Icons](https://icons.getbootstrap.com) | MIT | icons.getbootstrap.com |
| [Feather Icons](https://feathericons.com) | MIT | feathericons.com |
| [Ionicons](https://ionic.io/ionicons) (Outline) | MIT | ionic.io/ionicons |
| [Phosphor Icons](https://phosphoricons.com) | MIT | phosphoricons.com |
| [Octicons](https://primer.style/foundations/icons) | MIT | primer.style |
| [Iconoir](https://iconoir.com) | MIT | iconoir.com |

You can also download from **any public GitHub repository** containing SVG files using a custom URL.

## Documentation

See the full [User Guide](Documentation/USER_GUIDE.md) for detailed usage instructions, troubleshooting, and tips.

## Contributing

Contributions are welcome! Feel free to open issues and pull requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feat/my-feature`)
3. Commit using [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `docs:`, etc.)
4. Push and open a Pull Request

## License

This project is licensed under the [MIT License](LICENSE).

## Third-Party Notices

This asset references the following open-source icon libraries, which are downloaded on-demand (no icons are bundled):

| Library | License |
|---|---|
| Lucide | ISC |
| Tabler Icons | MIT |
| Heroicons | MIT |
| Bootstrap Icons | MIT |
| Feather Icons | MIT |
| Ionicons | MIT |
| Phosphor Icons | MIT |
| Octicons | MIT |
| Iconoir | MIT |

This asset uses Lucide under ISC license and Tabler Icons, Heroicons, Bootstrap Icons, Feather Icons, Ionicons, Phosphor Icons, Octicons, and Iconoir under MIT license; see [Third-Party Notices.txt](Third-Party%20Notices.txt) file in package for details.

The tool automatically downloads each library's license file alongside icons. Always verify the license of the icons you use in your project.