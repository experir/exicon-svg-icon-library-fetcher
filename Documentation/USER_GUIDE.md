# SVG Icon Fetcher - User Guide

## Overview
SVG Icon Fetcher is a Unity tool that allows you to easily download and import SVG icons from public repositories like Lucide and Tabler Icons.

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

**Note:** The first time you load a package, the tool downloads the list from GitHub. Subsequent times it will use the local cache (much faster!).

#### 3. Select Icons

- Scroll through the list and check the icons you want to download
- You can select as many icons as you want

#### 4. Download Icons

1. Set the destination folder in **"Output Folder"** (default: `Assets/Icons`)
2. Click **"Download Selected"**
3. The icons will be downloaded and automatically imported into your Unity project

### Method 2: Download from Custom URL

This method allows you to download a single icon directly from a GitHub link with just one click.

#### 1. Get the Icon URL

1. Navigate to an SVG icon on GitHub (e.g., browse Lucide, Tabler, or any GitHub repository with SVG icons)
2. Copy the URL from your browser
   - Example: `https://github.com/lucide-icons/lucide/blob/main/icons/heart.svg`
   - The URL should end with `.svg`

#### 2. Download the Icon

1. Paste the URL in the **"Icon URL"** field
2. Click **"Download Icon"**
3. The tool will automatically:
   - Download the SVG from GitHub
   - Sanitize and process the file
   - Save it to the output folder
   - Import it into Unity
4. A success dialog will confirm the download and show the save location
5. The URL field will be cleared automatically, ready for the next icon

**Supported URLs:**
- Direct GitHub links (blob URLs are automatically converted to raw URLs)
- Raw GitHub content URLs
- Any publicly accessible SVG file URL

## Cache Management

### Clear Cache
The **"Clear Cache"** button removes the local cache for the selected package.

**When to use it:**
- You want to update the icon list (if the repository has added new ones)
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
  - Make sure the URL ends with `.svg`
  - Verify the file is publicly accessible
  - Try copying the URL directly from the GitHub file page

### "Failed to load icon from URL"
- **Cause:** Network issue or invalid URL format
- **Solution:**
  - Check your internet connection
  - Ensure the URL is from a public GitHub repository
  - Try accessing the URL in your browser first

## Tips & Tricks

1. **First time:** Load all 3 packages and cache them for future use
2. **Organization:** Create different subfolders for each package (e.g., `Assets/Icons/Lucide`, `Assets/Icons/Tabler`)
3. **Backup:** Cache is located in `Library/SvgIconFetcher/` - you can copy it to other projects
4. **Custom URLs:** Use the custom URL method to quickly download single icons without browsing entire packs
5. **Mix and Match:** You can use both methods - browse packs for bulk downloads and custom URLs for specific icons

## Credits

- **Lucide Icons:** https://lucide.dev
- **Tabler Icons:** https://tabler.io/icons
- **Heroicons:** https://heroicons.com
- **Bootstrap Icons:** https://icons.getbootstrap.com
- **Feather Icons:** https://feathericons.com
- **Ionicons:** https://ionic.io/ionicons
- **Phosphor Icons:** https://phosphoricons.com
- **Octicons:** https://primer.style/foundations/icons
- **Iconoir:** https://iconoir.com
