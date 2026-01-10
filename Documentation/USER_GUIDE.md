# SVG Icon Fetcher - User Guide

## Overview
SVG Icon Fetcher is a Unity tool that allows you to easily download and import SVG icons from public repositories like Lucide and Tabler Icons.

## System Requirements

**Unity Version:** Unity 6.0 or later

**SVG Rendering Support:**
- **Unity 6.3+**: Built-in SVG support
- **Earlier versions** (pre-6.3): Use the [`com.unity.vectorgraphics`](https://docs.unity3d.com/Packages/com.unity.vectorgraphics@latest) package for SVG support

**Note:** SVG icons require Vector Graphics support to render properly in the editor and at runtime. Make sure you have the appropriate module or package installed for your Unity version.

## Opening the Tool

1. In Unity's menu, go to: **Tools > SVG Icon Fetcher**
2. A window will open with the tool's interface

## Basic Usage

### Method 1: Browse Icon Packs

#### 1. Select an Icon Pack

In the **"Icon Pack"** dropdown, you can choose from:
- **Lucide** - Modern icon set (ISC License)
- **Tabler Outline** - Outline icons (MIT License)
- **Tabler Filled** - Filled icons (MIT License)
- **Heroicons Outline** - Tailwind CSS outline icons (MIT License)
- **Heroicons Solid** - Tailwind CSS solid icons (MIT License)
- **Bootstrap Icons** - Official Bootstrap icon library (MIT License)
- **Feather Icons** - Simply beautiful open source icons (MIT License)
- **Ionicons Outline** - Premium designed icons by Ionic (MIT License)
- **Phosphor Icons** - Flexible icon family (MIT License)
- **Octicons** - GitHub's official icons (MIT License)
- **Iconoir** - Minimalist icon library (MIT License)

#### 2. Load Icon List

1. Click the **"Load Icons"** button
2. You'll see a progress bar during loading
3. After a few seconds, the list of available icons will appear below

**Note:** The first time you load a package, the tool downloads the list from GitHub. Subsequent times it will use the local cache (much faster!). The cache automatically expires after 1 hour to ensure you always have access to the latest icons.

#### 3. Select Icons

- Scroll through the list and check the icons you want to download
- You can select as many icons as you want
- Use the **"Select All"** checkbox to quickly select or deselect all icons

**⚠️ Note:** Downloading all icons from a pack can take some time (icons are downloaded in parallel batches of 10 to speed up the process). If you don't need all of them, it's recommended to select only the ones you actually need.

#### 4. Download Icons

1. Set the destination folder in **"Output Folder"** (default: `Assets/Icons`)
   - Icons will be automatically organized in subfolders by pack name (e.g., `Assets/Icons/Lucide`, `Assets/Icons/Tabler`)
2. Click **"Download Selected"**
3. The icons will be downloaded in parallel batches of 10 for faster performance
4. During download:
   - A progress message will show in the console
   - You can click **"Cancel Download"** to stop the process at any time
   - Already downloaded icons will be kept even if you cancel
5. The icons will be automatically imported into your Unity project

**Note:** The download process can be interrupted at any time. If you change your mind while downloading all icons, click "Cancel Download" and the tool will keep the icons already downloaded but stop downloading the remaining ones.

### Method 2: Download from Custom URL

This method allows you to download a single icon or an entire folder of icons directly from a GitHub link with just a few clicks.

#### 2A: Single Icon Download

1. Navigate to an SVG icon on GitHub (e.g., browse Lucide, Tabler, or any GitHub repository with SVG icons)
2. Copy the URL from your browser
   - Example: `https://github.com/lucide-icons/lucide/blob/main/icons/heart.svg`
   - The URL should end with `.svg`

**To download:**
1. Paste the URL in the **"URL"** field
2. Click **"Download Icon"**
3. The tool will automatically:
   - Download the SVG from GitHub
   - Sanitize and process the file
   - Detect the icon pack from the URL and save to the appropriate folder (e.g., `Assets/Icons/Lucide`)
   - Import it into Unity
4. A success dialog will confirm the download and show the save location
5. The URL field will be cleared automatically, ready for the next icon

**Supported URLs:**
- Direct GitHub links (blob URLs are automatically converted to raw URLs)
- Raw GitHub content URLs
- Any publicly accessible SVG file URL

#### 2B: Entire Folder Download (NEW)

Download multiple icons at once from a GitHub folder!

1. Navigate to a folder containing SVG icons on GitHub
   - Example: `https://github.com/tabler/tabler-icons/tree/main/icons/filled`
2. Copy the GitHub folder URL from your browser (make sure it contains `/tree/`)
3. Paste it in the **"URL"** field
4. Click **"Load Folder Contents"** - the tool will:
   - Fetch all SVG files in that folder from GitHub
   - Display a list of all icons found
5. Select which icons you want to download (use "Select All" for convenience)
6. Optionally customize the **"Output Subfolder Name"** (defaults to the folder name)
7. Click **"Download Selected"** to download your chosen icons in parallel

**Supported Folder URLs:**
- GitHub browser URLs with `/tree/` in the path
- Works with any public GitHub repository
- Example formats:
  - `https://github.com/tabler/tabler-icons/tree/main/icons/filled`
  - `https://github.com/lucide-icons/lucide/tree/main/icons`
  - `https://github.com/your-org/your-repo/tree/branch/path/to/icons`

**Benefits:**
- Download 10+ icons in seconds with parallel downloading
- Selective download - choose only the icons you need
- Automatic folder organization - icons are saved with the folder's name
- No manual file selection required

## Cache Management

### Clear Cache
The **"Clear Cache"** button removes the local cache for the selected package.

**Automatic Cache Expiration:**
- Cache automatically expires after 1 hour
- This ensures you always get the latest icons from the repositories
- Helps stay within GitHub's rate limits while keeping data fresh

**Manual Clear - When to use it:**
- You want to immediately refresh the icon list (without waiting for the 1-hour expiration)
- You have problems with corrupted cache

**Note:** After clearing the cache, the next "Load Icons" will download the list from GitHub again.

## Limits and Considerations

### GitHub Rate Limit
GitHub allows 60 API requests per hour without authentication.

**What this means:**
- You can load up to 60 different packages per hour
- Once a package is loaded, it's saved to cache
- Cache doesn't count toward the rate limit

**If you hit the limit:**
1. Wait 1 hour for automatic reset
2. Use existing cache (don't click "Clear Cache")
3. The error message will tell you when the limit resets

### Performance
- **First request:** 1-2 seconds (download from GitHub)
- **Subsequent requests:** Instant (uses cache)
- **Folder download:** Depends on number of icons (10 icons in ~2-3 seconds with parallel downloading)

## Licenses

Always verify the license of the icon package you're using:
- **Lucide:** ISC License
- **Tabler Icons:** MIT License
- **Heroicons:** MIT License (by Tailwind Labs)
- **Bootstrap Icons:** MIT License
- **Feather Icons:** MIT License
- **Ionicons:** MIT License
- **Phosphor Icons:** MIT License
- **Octicons:** MIT License (by GitHub)
- **Iconoir:** MIT License

All licenses allow commercial use. It's good practice to credit the authors.

## Troubleshooting

### "GitHub API rate limit reached"
- **Cause:** You've made more than 60 requests in one hour
- **Solution:** Wait for reset (the error tells you when) or use the cache

### "No cache found"
- **Cause:** You've never loaded that package before
- **Solution:** Click "Load Icons" to download the list

### Icons don't appear after download
- **Solution:** Wait a few seconds, Unity imports files automatically
- If they still don't appear, right-click in the Project panel and select "Refresh"

### Custom URL doesn't work
- **Cause:** Invalid URL or inaccessible file
- **Solution:** 
  - For single icons: Make sure the URL ends with `.svg`
  - For folders: Make sure the URL contains `/tree/` (not `/blob/`)
  - Verify the file/folder is publicly accessible
  - Try copying the URL directly from the GitHub page

### Folder URL not recognized
- **Cause:** URL doesn't contain `/tree/` (it's a file URL, not a folder URL)
- **Solution:** 
  - Navigate to a folder in GitHub (not a file)
  - Copy the folder URL from the browser
  - Make sure the URL contains `/tree/branch-name/path`
  - Example: `https://github.com/tabler/tabler-icons/tree/main/icons/filled`

### "Failed to load icon from URL"
- **Cause:** Network issue or invalid URL format
- **Solution:**
  - Check your internet connection
  - Ensure the URL is from a public GitHub repository
  - Try accessing the URL in your browser first

## Tips & Tricks

1. **First time:** Load icon packs and cache them for future use
2. **Organization:** Icons are automatically organized in subfolders by pack name (e.g., `Assets/Icons/Lucide`, `Assets/Icons/Tabler`)
3. **Cache:** Icon lists are cached for 1 hour in `Library/SvgIconFetcher/` for faster loading and to stay within API limits
4. **Custom URLs:** Use the custom URL method to quickly download single icons or entire folders without browsing entire packs
5. **Folder Downloads:** Copy a GitHub folder URL (with `/tree/`) and download all icons at once - perfect for getting a subset of a larger icon pack
6. **Mix and Match:** You can use all three methods - browse full packs, download single icons, or download custom folders
7. **Cancel Downloads:** If you start downloading icons but change your mind, click "Cancel Download" to stop while keeping already downloaded icons
8. **Parallel Downloads:** Icons are downloaded in batches of 10 simultaneously for faster performance
9. **Custom Subfolder Names:** When downloading folders, customize the output folder name to organize your icons however you like
- **Heroicons:** https://heroicons.com
- **Bootstrap Icons:** https://icons.getbootstrap.com
- **Feather Icons:** https://feathericons.com
- **Ionicons:** https://ionic.io/ionicons
- **Phosphor Icons:** https://phosphoricons.com
- **Octicons:** https://primer.style/foundations/icons
- **Iconoir:** https://iconoir.com
