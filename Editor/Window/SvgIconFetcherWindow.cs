using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using SvgIconFetcher.Models;
using SvgIconFetcher.Core;

namespace SvgIconFetcher.Window
{
    public class SvgIconFetcherWindow : EditorWindow
    {
        private int selectedSourceIndex = 0;
        private List<string> icons = new();
        private HashSet<string> selectedIcons = new();
        private Vector2 mainScroll;
        private Vector2 scroll;
        private string outputFolder = "Assets/Icons";
        private bool isLoading = false;
        private string loadingMessage = "";
        private bool isDownloading = false;
        private bool cancelDownload = false;
        
        // Custom URL mode
        private string customUrl = "";
        private string customIconPreview = "";
        private string customIconName = "";
        private bool isLoadingCustom = false;
        private Vector2 customPreviewScroll;
        private string customResultMessage = "";
        private MessageType customResultType = MessageType.None;
        
        // Search filters
        private string searchFilter = "";
        private string folderSearchFilter = "";
        
        // Folder mode
        private List<string> folderIcons = new();
        private HashSet<string> selectedFolderIcons = new();
        private Vector2 folderScroll;
        private string folderUrl = "";
        private bool isLoadingFolder = false;
        private string folderLoadingMessage = "";
        private bool isDownloadingFolder = false;
        private bool cancelFolderDownload = false;
        private string folderOutputName = "";

        // PNG export options
#if EXICON_VECTORGRAPHICS
        private bool exportAsPng = false;
        private int pngSizeIndex = 3;
        private readonly int[] pngSizes = { 32, 64, 128, 256, 512, 1024 };
        private readonly string[] pngSizeLabels = { "32×32", "64×64", "128×128", "256×256", "512×512", "1024×1024" };
#endif

        // Color override
        private bool overrideColor = false;
        private Color overrideColorValue = Color.white;

        // Inline notification (replaces popup dialogs)
        private string notificationMessage = "";
        private MessageType notificationType = MessageType.Info;
        private double notificationExpireTime = 0;

        [MenuItem("Tools/Exicon - SVG Icon Fetcher")]
        public static void Open()
        {
            var window = GetWindow<SvgIconFetcherWindow>("Exicon - SVG Icon Fetcher");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            // Load saved folder state when window opens
            LoadFolderState();

            // Load PNG export preferences
#if EXICON_VECTORGRAPHICS
            exportAsPng = EditorPrefs.GetBool("SvgIconFetcher_ExportAsPng", false);
            pngSizeIndex = EditorPrefs.GetInt("SvgIconFetcher_PngSizeIndex", 3);
#endif

            // Load color override preferences
            overrideColor = EditorPrefs.GetBool("SvgIconFetcher_OverrideColor", false);
            if (ColorUtility.TryParseHtmlString(EditorPrefs.GetString("SvgIconFetcher_OverrideColorHex", "#FFFFFF"), out var saved))
                overrideColorValue = saved;
        }

        private void OnGUI()
        {
            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            
            // Method 1: Icon Packs
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Method 1: Browse Icon Packs", EditorStyles.boldLabel);

            var sources = IconSourceRegistry.Sources;
            selectedSourceIndex = EditorGUILayout.Popup(
                "Icon Pack",
                selectedSourceIndex,
                sources.ConvertAll(s => s.Name).ToArray()
            );

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(isLoading);
            if (GUILayout.Button(isLoading ? "Loading..." : "Load Icons"))
                LoadIcons();
            if (GUILayout.Button("Clear Cache"))
                ClearCache();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Inline notification banner
            DrawNotification();

            // Loading indicator
            if (isLoading)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(loadingMessage, MessageType.Info);
                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                EditorGUI.ProgressBar(rect, -1, "Loading...");
                Repaint(); // Force repaint during async operation
            }

            EditorGUILayout.Space(10);
            
            // Build filtered list
            List<string> filteredIcons = icons;
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filteredIcons = icons.FindAll(i => i.IndexOf(searchFilter, System.StringComparison.OrdinalIgnoreCase) >= 0);
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);
            
            // Select All / Deselect All toggle
            if (icons.Count > 0)
            {
                bool allSelected = filteredIcons.Count > 0 && filteredIcons.TrueForAll(i => selectedIcons.Contains(i));
                bool newAllSelected = EditorGUILayout.ToggleLeft("Select All", allSelected, GUILayout.Width(100));
                
                if (newAllSelected != allSelected)
                {
                    if (newAllSelected)
                    {
                        foreach (var icon in filteredIcons)
                            selectedIcons.Add(icon);
                    }
                    else
                    {
                        foreach (var icon in filteredIcons)
                            selectedIcons.Remove(icon);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Warning about download time
            if (icons.Count > 0)
            {
                EditorGUILayout.HelpBox("Downloading all icons may take some time. Consider selecting only the ones you need.", MessageType.Info);
            }
            
            // Search bar
            if (icons.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search", GUILayout.Width(50));
                searchFilter = EditorGUILayout.TextField(searchFilter);
                if (GUILayout.Button("✕", GUILayout.Width(22)))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(searchFilter))
                    EditorGUILayout.LabelField($"Showing {filteredIcons.Count} of {icons.Count} icons", EditorStyles.miniLabel);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
            foreach (var icon in filteredIcons)
            {
                bool selected = selectedIcons.Contains(icon);
                bool newSelected = EditorGUILayout.ToggleLeft(icon, selected);

                if (newSelected)
                    selectedIcons.Add(icon);
                else
                    selectedIcons.Remove(icon);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            DrawPngExportOptions();

            if (!isDownloading)
            {
                if (GUILayout.Button("Download Selected"))
                    DownloadSelected();
            }
            else
            {
                EditorGUILayout.HelpBox("Download in progress... Click 'Cancel' to stop.", MessageType.Warning);
                if (GUILayout.Button("Cancel Download"))
                    cancelDownload = true;
            }
                
            // Method 2: Custom URL (Single Icon or Folder)
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Method 2: Download from Custom URL", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Paste a link to an SVG icon or entire folder from GitHub.\n" +
                "Single icon: https://github.com/lucide-icons/lucide/blob/main/icons/heart.svg\n" +
                "Folder: https://github.com/tabler/tabler-icons/tree/main/icons/filled",
                MessageType.Info
            );
            
            customUrl = EditorGUILayout.TextField("URL", customUrl);
            
            // Show appropriate UI based on URL type
            if (!string.IsNullOrEmpty(customUrl) && IsFolderUrl(customUrl))
            {
                EditorGUILayout.HelpBox("✓ Folder detected! Click the button below to load the icons.", MessageType.Info);
                DrawFolderModeUI();
            }
            else if (!string.IsNullOrEmpty(customUrl))
            {
                EditorGUILayout.HelpBox("✓ Single file detected! Use the button below.", MessageType.Info);
                DrawSingleIconModeUI();
            }
            else
            {
                DrawSingleIconModeUI();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawSingleIconModeUI()
        {
            DrawPngExportOptions();
            EditorGUI.BeginDisabledGroup(isLoadingCustom || string.IsNullOrEmpty(customUrl));
            if (GUILayout.Button(isLoadingCustom ? "Downloading..." : "Download Icon"))
                DownloadCustomIcon();
            EditorGUI.EndDisabledGroup();
            
            // Show result message after download
            if (!string.IsNullOrEmpty(customResultMessage))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(customResultMessage, customResultType);
            }
        }

        private void DrawFolderModeUI()
        {
            // Set the folderUrl from customUrl
            folderUrl = customUrl;
            
            EditorGUILayout.Space(10);
            
            // Load folder contents button with prominent styling
            EditorGUILayout.LabelField("Step 1: Load Folder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click the button to load the list of icons from the folder.", MessageType.Info);
            
            EditorGUI.BeginDisabledGroup(isLoadingFolder);
            if (GUILayout.Button(isLoadingFolder ? "Loading..." : "Load Folder Contents", GUILayout.Height(35)))
                LoadFolderIcons();
            EditorGUI.EndDisabledGroup();
            
            // Loading indicator
            if (isLoadingFolder)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(folderLoadingMessage, MessageType.Info);
                var rect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                EditorGUI.ProgressBar(rect, -1, "Loading...");
            }
            
            // Show loaded icons
            if (folderIcons.Count > 0)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("Step 2: Select Icons", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox($"Found {folderIcons.Count} SVG files", MessageType.Info);
                
                // Build filtered folder list
                List<string> filteredFolderIcons = folderIcons;
                if (!string.IsNullOrEmpty(folderSearchFilter))
                {
                    filteredFolderIcons = folderIcons.FindAll(i => i.IndexOf(folderSearchFilter, System.StringComparison.OrdinalIgnoreCase) >= 0);
                }
                
                // Select All / Deselect All
                EditorGUILayout.BeginHorizontal();
                bool allSelected = filteredFolderIcons.Count > 0 && filteredFolderIcons.TrueForAll(i => selectedFolderIcons.Contains(i));
                bool newAllSelected = EditorGUILayout.ToggleLeft("Select All", allSelected, GUILayout.Width(100));
                
                if (newAllSelected != allSelected)
                {
                    if (newAllSelected)
                    {
                        foreach (var icon in filteredFolderIcons)
                            selectedFolderIcons.Add(icon);
                    }
                    else
                    {
                        foreach (var icon in filteredFolderIcons)
                            selectedFolderIcons.Remove(icon);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                // Search bar
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search", GUILayout.Width(50));
                folderSearchFilter = EditorGUILayout.TextField(folderSearchFilter);
                if (GUILayout.Button("✕", GUILayout.Width(22)))
                {
                    folderSearchFilter = "";
                    GUI.FocusControl(null);
                }
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(folderSearchFilter))
                    EditorGUILayout.LabelField($"Showing {filteredFolderIcons.Count} of {folderIcons.Count} icons", EditorStyles.miniLabel);
                
                // Icon list
                folderScroll = EditorGUILayout.BeginScrollView(folderScroll, GUILayout.Height(200));
                foreach (var icon in filteredFolderIcons)
                {
                    bool selected = selectedFolderIcons.Contains(icon);
                    bool newSelected = EditorGUILayout.ToggleLeft(icon, selected);
                    
                    if (newSelected)
                        selectedFolderIcons.Add(icon);
                    else
                        selectedFolderIcons.Remove(icon);
                }
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("Step 3: Download", EditorStyles.boldLabel);
                
                // Output folder and download
                folderOutputName = EditorGUILayout.TextField("Output Subfolder Name", folderOutputName);
                EditorGUILayout.HelpBox("Icons will be saved to: Assets/Icons/" + (string.IsNullOrEmpty(folderOutputName) ? "[folder name]" : folderOutputName), MessageType.Info);
                DrawPngExportOptions();
                
                if (!isDownloadingFolder)
                {
                    if (GUILayout.Button("Download Selected", GUILayout.Height(35)))
                        DownloadFolderIcons();
                }
                else
                {
                    EditorGUILayout.HelpBox("Download in progress... Click 'Cancel' to stop.", MessageType.Warning);
                    if (GUILayout.Button("Cancel Download"))
                        cancelFolderDownload = true;
                }
            }
        }

        private void DrawPngExportOptions()
        {
#if EXICON_VECTORGRAPHICS
            EditorGUILayout.BeginHorizontal();
            bool newExportAsPng = EditorGUILayout.Toggle("Also Export as PNG", exportAsPng);
            if (newExportAsPng != exportAsPng)
            {
                exportAsPng = newExportAsPng;
                EditorPrefs.SetBool("SvgIconFetcher_ExportAsPng", exportAsPng);
            }
            EditorGUI.BeginDisabledGroup(!exportAsPng);
            int newSizeIndex = EditorGUILayout.Popup(pngSizeIndex, pngSizeLabels);
            if (newSizeIndex != pngSizeIndex)
            {
                pngSizeIndex = newSizeIndex;
                EditorPrefs.SetInt("SvgIconFetcher_PngSizeIndex", pngSizeIndex);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
#else
            EditorGUILayout.HelpBox(
                "PNG export requires the com.unity.vectorgraphics package.\n" +
                "• Unity < 6.3: install com.unity.vectorgraphics 2.0.0-preview.21\n" +
                "• Unity ≥ 6.3: install com.unity.vectorgraphics 3.0.0-pre.2\n\n" +
                "Add it via Window → Package Manager → + → Add by name.",
                MessageType.Info);
#endif

            // Color override (applies to SVG content before saving; affects PNG too)
            EditorGUILayout.BeginHorizontal();
            bool newOverrideColor = EditorGUILayout.Toggle("Override Color", overrideColor);
            if (newOverrideColor != overrideColor)
            {
                overrideColor = newOverrideColor;
                EditorPrefs.SetBool("SvgIconFetcher_OverrideColor", overrideColor);
            }
            EditorGUI.BeginDisabledGroup(!overrideColor);
            Color newColor = EditorGUILayout.ColorField(GUIContent.none, overrideColorValue, false, false, false);
            if (newColor != overrideColorValue)
            {
                overrideColorValue = newColor;
                EditorPrefs.SetString("SvgIconFetcher_OverrideColorHex", "#" + ColorUtility.ToHtmlStringRGB(overrideColorValue));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Applies color override to SVG content if the option is enabled.
        /// </summary>
        private string ApplyColorOverride(string svg)
        {
            if (!overrideColor)
                return svg;
            return SvgSanitizer.OverrideColor(svg, "#" + ColorUtility.ToHtmlStringRGB(overrideColorValue));
        }

        /// <summary>
        /// Sets up the PNG texture importer so alpha is treated as transparency.
        /// </summary>
#if EXICON_VECTORGRAPHICS
        private static void ConfigurePngImporter(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.SaveAndReimport();
            }
        }
#endif

        private async void LoadIcons()
        {
            if (isLoading) return;

            icons.Clear();
            selectedIcons.Clear();
            isLoading = true;
            loadingMessage = "Fetching icon list from GitHub...";

            try
            {
                var source = IconSourceRegistry.Sources[selectedSourceIndex];
                icons = await IconIndexFetcher.FetchIcons(source);
                
                // Update output folder to include pack name
                outputFolder = $"Assets/Icons/{source.Name}";
                
                loadingMessage = $"✓ Loaded {icons.Count} icons from {source.Name}";
                Debug.Log($"Loaded {icons.Count} icons from {source.Name}");
                
                // Show success message briefly
                await System.Threading.Tasks.Task.Delay(1500);
            }
            catch (System.Exception e)
            {
                loadingMessage = "";
                ShowNotification($"Error loading icons: {e.Message}", MessageType.Error);
            }
            finally
            {
                isLoading = false;
                Repaint();
            }
        }

        private void ClearCache()
        {
            var source = IconSourceRegistry.Sources[selectedSourceIndex];
            var cachePath = IconIndexCache.GetCachePath(source.Name);
            
            if (System.IO.File.Exists(cachePath))
            {
                System.IO.File.Delete(cachePath);
                Debug.Log($"Cache cleared for {source.Name}");
                ShowNotification($"Cache cleared for {source.Name}. You can now reload icons.", MessageType.Info);
            }
            else
            {
                ShowNotification("No cache found for this icon pack.", MessageType.Warning);
            }
        }

        // ── Inline notification helpers ──────────────────────────────────

        private void ShowNotification(string message, MessageType type, float durationSeconds = 5f)
        {
            notificationMessage = message;
            notificationType = type;
            notificationExpireTime = EditorApplication.timeSinceStartup + durationSeconds;
            Repaint();
        }

        private void DrawNotification()
        {
            if (string.IsNullOrEmpty(notificationMessage))
                return;

            if (EditorApplication.timeSinceStartup > notificationExpireTime)
            {
                notificationMessage = "";
                Repaint();
                return;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(notificationMessage, notificationType);
            if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(38)))
            {
                notificationMessage = "";
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            // Keep repainting to auto-dismiss when time expires
            Repaint();
        }

        // ─────────────────────────────────────────────────────────────────

        private async void DownloadSelected()
        {
            if (isDownloading) return;
            
            isDownloading = true;
            cancelDownload = false;
            
            var source = IconSourceRegistry.Sources[selectedSourceIndex];
            int totalIcons = selectedIcons.Count;
            int downloadedCount = 0;
            int failedCount = 0;
            
            // Convert HashSet to List for batch processing
            var iconList = new List<string>(selectedIcons);
            
            // Download icons in parallel (batches of 10 concurrent downloads)
            const int batchSize = 10;
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();
            
            for (int i = 0; i < iconList.Count; i++)
            {
                // Check if user cancelled
                if (cancelDownload)
                {
                    Debug.LogWarning($"Download cancelled by user. Downloaded {downloadedCount} icons before cancellation.");
                    break;
                }
                
                var icon = iconList[i];
                var downloadTask = DownloadIconAsync(source, icon, outputFolder);
                
                _ = downloadTask.ContinueWith(task =>
                {
                    if (task.Result)
                    {
                        downloadedCount++;
                        Debug.Log($"✓ Downloaded ({downloadedCount}/{totalIcons}): {icon}");
                    }
                    else
                    {
                        failedCount++;
                        Debug.LogError($"✗ Failed ({failedCount} failures): {icon}");
                    }
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                
                tasks.Add(downloadTask);
                
                // Wait for batch to complete before starting next batch
                if (tasks.Count >= batchSize || i == iconList.Count - 1)
                {
                    await System.Threading.Tasks.Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            
            AssetDatabase.Refresh();
            
            if (cancelDownload)
                Debug.Log($"Download cancelled: {downloadedCount} succeeded, {failedCount} failed, {totalIcons - downloadedCount - failedCount} skipped");
            else
                Debug.Log($"Download complete: {downloadedCount} succeeded, {failedCount} failed");
            
            isDownloading = false;
            cancelDownload = false;
            Repaint();
        }
        
        private async System.Threading.Tasks.Task<bool> DownloadIconAsync(IconSource source, string icon, string folder)
        {
            try
            {
                var url = $"{source.BaseUrl}/{icon}.svg";
                var svg = await SvgDownloader.Download(url);
                svg = SvgSanitizer.Sanitize(svg);
                svg = ApplyColorOverride(svg);

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                var path = System.IO.Path.Combine(folder, $"{icon}.svg");
                System.IO.File.WriteAllText(path, svg);
                AssetDatabase.ImportAsset(path);

                // Export as PNG if enabled
#if EXICON_VECTORGRAPHICS
                if (exportAsPng)
                {
                    var pngPath = System.IO.Path.Combine(folder, $"{icon}.png");
                    SvgToPngConverter.SavePng(svg, pngPath, pngSizes[pngSizeIndex]);
                    ConfigurePngImporter(pngPath);
                }
#endif
                
                // Try to download license file from the repository
                var githubInfo = ExtractGitHubInfo(source.BaseUrl);
                if (githubInfo.HasValue && !System.IO.File.Exists(System.IO.Path.Combine(folder, "LICENSE")))
                {
                    var licenseContent = await SvgDownloader.TryDownloadLicense(githubInfo.Value.owner, githubInfo.Value.repo, githubInfo.Value.branch);
                    if (!string.IsNullOrEmpty(licenseContent))
                    {
                        var licensePath = System.IO.Path.Combine(folder, "LICENSE");
                        System.IO.File.WriteAllText(licensePath, licenseContent);
                        AssetDatabase.ImportAsset(licensePath);
                    }
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error downloading '{icon}': {e.Message}");
                return false;
            }
        }
        
        private async void DownloadCustomIcon()
        {
            if (isLoadingCustom || string.IsNullOrEmpty(customUrl))
                return;
            
            isLoadingCustom = true;
            customIconPreview = "";
            customIconName = "";
            customResultMessage = "";
            customResultType = MessageType.None;
            
            try
            {
                // Convert GitHub blob URL to raw URL
                string rawUrl = customUrl
                    .Replace("github.com", "raw.githubusercontent.com")
                    .Replace("/blob/", "/");
                
                // Extract icon name from URL
                var uri = new System.Uri(rawUrl);
                customIconName = System.IO.Path.GetFileNameWithoutExtension(uri.AbsolutePath);
                
                // Detect pack name from URL
                string packName = DetectPackFromUrl(customUrl);
                string customOutputFolder = string.IsNullOrEmpty(packName) 
                    ? "Assets/Icons" 
                    : $"Assets/Icons/{packName}";
                
                // Download and sanitize SVG
                var svg = await SvgDownloader.Download(rawUrl);
                customIconPreview = SvgSanitizer.Sanitize(svg);
                customIconPreview = ApplyColorOverride(customIconPreview);
                
                // Save to disk
                if (!System.IO.Directory.Exists(customOutputFolder))
                    System.IO.Directory.CreateDirectory(customOutputFolder);
                
                var path = System.IO.Path.Combine(customOutputFolder, $"{customIconName}.svg");
                System.IO.File.WriteAllText(path, customIconPreview);
                AssetDatabase.ImportAsset(path);

                // Export as PNG if enabled
#if EXICON_VECTORGRAPHICS
                if (exportAsPng)
                {
                    var pngPath = System.IO.Path.Combine(customOutputFolder, $"{customIconName}.png");
                    SvgToPngConverter.SavePng(customIconPreview, pngPath, pngSizes[pngSizeIndex]);
                    ConfigurePngImporter(pngPath);
                }
#endif
                
                // Try to download license file from the repository
                var githubInfo = ExtractGitHubInfo(customUrl);
                if (githubInfo.HasValue && !System.IO.File.Exists(System.IO.Path.Combine(customOutputFolder, "LICENSE")))
                {
                    var licenseContent = await SvgDownloader.TryDownloadLicense(githubInfo.Value.owner, githubInfo.Value.repo, githubInfo.Value.branch);
                    if (!string.IsNullOrEmpty(licenseContent))
                    {
                        var licensePath = System.IO.Path.Combine(customOutputFolder, "LICENSE");
                        System.IO.File.WriteAllText(licensePath, licenseContent);
                        AssetDatabase.ImportAsset(licensePath);
                    }
                }
                
                AssetDatabase.Refresh();
                
                Debug.Log($"Downloaded custom icon: {customIconName} to {customOutputFolder}");
                customResultMessage = $"✓ Icon '{customIconName}.svg' downloaded successfully to:\n{customOutputFolder}";
                customResultType = MessageType.Info;
            }
            catch (System.Exception e)
            {
                customResultMessage = $"Failed to download icon:\n{e.Message}\n\nMake sure the URL is a direct link to an SVG file on GitHub.";
                customResultType = MessageType.Error;
            }
            finally
            {
                isLoadingCustom = false;
                Repaint();
            }
        }
        
        /// <summary>
        /// Determines if a URL is a GitHub folder URL (contains /tree/) or a single file URL
        /// </summary>
        private bool IsFolderUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.Contains("/tree/");
        }
        
        private string DetectPackFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            
            url = url.ToLower();
            
            // Check for known repositories
            if (url.Contains("lucide-icons/lucide"))
                return "Lucide";
            
            if (url.Contains("tabler/tabler-icons"))
            {
                if (url.Contains("/filled"))
                    return "Tabler Filled";
                else if (url.Contains("/outline"))
                    return "Tabler Outline";
                return "Tabler Outline"; // Default to outline
            }
            
            if (url.Contains("tailwindlabs/heroicons"))
            {
                if (url.Contains("/solid"))
                    return "Heroicons Solid";
                else if (url.Contains("/outline"))
                    return "Heroicons Outline";
                return "Heroicons Outline"; // Default to outline
            }
            
            if (url.Contains("twbs/icons"))
                return "Bootstrap Icons";
            
            if (url.Contains("feathericons/feather"))
                return "Feather Icons";
            
            if (url.Contains("ionic-team/ionicons"))
                return "Ionicons Outline";
            
            if (url.Contains("phosphor-icons/core"))
                return "Phosphor Icons";
            
            if (url.Contains("primer/octicons"))
                return "Octicons";
            
            if (url.Contains("iconoir-icons/iconoir"))
                return "Iconoir";
            
            // Unknown repository - extract repository name from GitHub URL
            // URL format: https://github.com/{owner}/{repo}/...
            try
            {
                var uri = new System.Uri(url);
                if (uri.Host.Contains("github.com") || uri.Host.Contains("githubusercontent.com"))
                {
                    var pathSegments = uri.AbsolutePath.Split('/');
                    if (pathSegments.Length >= 3)
                    {
                        // pathSegments[0] is empty, pathSegments[1] is owner, pathSegments[2] is repo
                        string repoName = pathSegments[2];
                        // Capitalize first letter for better folder naming
                        if (!string.IsNullOrEmpty(repoName))
                            return char.ToUpper(repoName[0]) + repoName.Substring(1);
                    }
                }
            }
            catch
            {
                // If URL parsing fails, return null
            }
            
            return null;
        }

        private async void LoadFolderIcons()
        {
            if (isLoadingFolder || string.IsNullOrEmpty(folderUrl))
                return;

            folderIcons.Clear();
            selectedFolderIcons.Clear();
            isLoadingFolder = true;
            folderLoadingMessage = "Fetching folder contents from GitHub...";

            try
            {
                folderIcons = await FolderIndexFetcher.FetchSvgsFromFolder(folderUrl);
                
                // Extract folder path and package name from URL
                var parsed = FolderIndexFetcher.ParseGitHubFolderUrl(folderUrl);
                if (parsed.HasValue && !string.IsNullOrEmpty(parsed.Value.folderPath))
                {
                    // Get package name from repo (e.g., "tabler-icons" -> "tabler")
                    string packageName = parsed.Value.repo;
                    if (packageName.EndsWith("-icons"))
                        packageName = packageName.Substring(0, packageName.Length - 6);
                    
                    // Get folder path without "icons/" prefix
                    string path = parsed.Value.folderPath.TrimStart('/').TrimEnd('/');
                    if (path.StartsWith("icons/"))
                    {
                        path = path.Substring(6); // Remove "icons/"
                    }
                    
                    folderOutputName = string.IsNullOrEmpty(path) ? packageName : $"{packageName}/{path}";
                }
                
                folderLoadingMessage = $"✓ Loaded {folderIcons.Count} SVG files from folder";
                Debug.Log($"Loaded {folderIcons.Count} SVG files from folder");
                
                // Save state so it persists when reopening the window
                SaveFolderState();
                
                // Show success message briefly
                await System.Threading.Tasks.Task.Delay(1500);
            }
            catch (System.Exception e)
            {
                folderLoadingMessage = "";
                ShowNotification($"Error loading folder: {e.Message}", MessageType.Error);
            }
            finally
            {
                isLoadingFolder = false;
                Repaint();
            }
        }

        private async void DownloadFolderIcons()
        {
            if (isDownloadingFolder || folderIcons.Count == 0)
                return;

            isDownloadingFolder = true;
            cancelFolderDownload = false;

            // Parse URL to get base download path
            var parsed = FolderIndexFetcher.ParseGitHubFolderUrl(folderUrl);
            if (!parsed.HasValue)
            {
                ShowNotification("Invalid folder URL.", MessageType.Error);
                isDownloadingFolder = false;
                return;
            }

            var (owner, repo, branch, folderPath) = parsed.Value;
            
            int totalIcons = selectedFolderIcons.Count;
            int downloadedCount = 0;
            int failedCount = 0;

            // Convert HashSet to List for batch processing
            var iconList = new List<string>(selectedFolderIcons);

            // Determine output folder using full folder path
            string outputFolder = $"Assets/Icons/{folderOutputName}";

            // Download icons in parallel (batches of 10 concurrent downloads)
            const int batchSize = 10;
            var tasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

            for (int i = 0; i < iconList.Count; i++)
            {
                // Check if user cancelled
                if (cancelFolderDownload)
                {
                    Debug.LogWarning($"Download cancelled by user. Downloaded {downloadedCount} icons before cancellation.");
                    break;
                }

                var icon = iconList[i];
                var downloadTask = DownloadFolderIconAsync(owner, repo, branch, folderPath, icon, outputFolder);

                _ = downloadTask.ContinueWith(task =>
                {
                    if (task.Result)
                    {
                        downloadedCount++;
                        Debug.Log($"✓ Downloaded ({downloadedCount}/{totalIcons}): {icon}");
                    }
                    else
                    {
                        failedCount++;
                        Debug.LogError($"✗ Failed ({failedCount} failures): {icon}");
                    }
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

                tasks.Add(downloadTask);

                // Wait for batch to complete before starting next batch
                if (tasks.Count >= batchSize || i == iconList.Count - 1)
                {
                    await System.Threading.Tasks.Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            AssetDatabase.Refresh();

            if (cancelFolderDownload)
                Debug.Log($"Download cancelled: {downloadedCount} succeeded, {failedCount} failed, {totalIcons - downloadedCount - failedCount} skipped");
            else
                Debug.Log($"Download complete: {downloadedCount} succeeded, {failedCount} failed");

            isDownloadingFolder = false;
            cancelFolderDownload = false;
            SaveFolderState(); // Save selections after download completes
            Repaint();
        }

        private async System.Threading.Tasks.Task<bool> DownloadFolderIconAsync(string owner, string repo, string branch, string folderPath, string icon, string folder)
        {
            try
            {
                // Build download URL
                string iconPath = string.IsNullOrEmpty(folderPath) 
                    ? $"{icon}.svg" 
                    : $"{folderPath}/{icon}.svg";
                
                string downloadUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{iconPath}";
                
                var svg = await SvgDownloader.Download(downloadUrl);
                svg = SvgSanitizer.Sanitize(svg);
                svg = ApplyColorOverride(svg);

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                var path = System.IO.Path.Combine(folder, $"{icon}.svg");
                System.IO.File.WriteAllText(path, svg);
                AssetDatabase.ImportAsset(path);

                // Export as PNG if enabled
#if EXICON_VECTORGRAPHICS
                if (exportAsPng)
                {
                    var pngPath = System.IO.Path.Combine(folder, $"{icon}.png");
                    SvgToPngConverter.SavePng(svg, pngPath, pngSizes[pngSizeIndex]);
                    ConfigurePngImporter(pngPath);
                }
#endif
                
                // Try to download license file from the repository (only once, check if it already exists)
                if (!System.IO.File.Exists(System.IO.Path.Combine(folder, "LICENSE")))
                {
                    var licenseContent = await SvgDownloader.TryDownloadLicense(owner, repo, branch);
                    if (!string.IsNullOrEmpty(licenseContent))
                    {
                        var licensePath = System.IO.Path.Combine(folder, "LICENSE");
                        System.IO.File.WriteAllText(licensePath, licenseContent);
                        AssetDatabase.ImportAsset(licensePath);
                    }
                }
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error downloading '{icon}': {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Save folder state to EditorPrefs for persistence
        /// </summary>
        private void SaveFolderState()
        {
            EditorPrefs.SetString("SvgIconFetcher_FolderUrl", folderUrl);
            EditorPrefs.SetString("SvgIconFetcher_FolderOutputName", folderOutputName);
            EditorPrefs.SetString("SvgIconFetcher_FolderIcons", string.Join("|", folderIcons));
            EditorPrefs.SetString("SvgIconFetcher_SelectedFolderIcons", string.Join("|", selectedFolderIcons));
        }
        
        /// <summary>
        /// Load folder state from EditorPrefs
        /// </summary>
        private void LoadFolderState()
        {
            folderUrl = EditorPrefs.GetString("SvgIconFetcher_FolderUrl", "");
            folderOutputName = EditorPrefs.GetString("SvgIconFetcher_FolderOutputName", "");
            
            string iconsStr = EditorPrefs.GetString("SvgIconFetcher_FolderIcons", "");
            folderIcons.Clear();
            if (!string.IsNullOrEmpty(iconsStr))
            {
                folderIcons.AddRange(iconsStr.Split('|'));
            }
            
            string selectedStr = EditorPrefs.GetString("SvgIconFetcher_SelectedFolderIcons", "");
            selectedFolderIcons.Clear();
            if (!string.IsNullOrEmpty(selectedStr))
            {
                foreach (var icon in selectedStr.Split('|'))
                {
                    if (!string.IsNullOrEmpty(icon))
                        selectedFolderIcons.Add(icon);
                }
            }
        }
        
        /// <summary>
        /// Extract GitHub owner, repo, and branch information from a GitHub URL
        /// </summary>
        /// <param name="url">GitHub URL (can be a blob or tree URL)</param>
        /// <returns>Tuple of (owner, repo, branch) or null if not a valid GitHub URL</returns>
        private (string owner, string repo, string branch)? ExtractGitHubInfo(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            
            try
            {
                // Handle both github.com and raw.githubusercontent.com URLs
                var uri = new System.Uri(url);
                if (!uri.Host.Contains("github.com") && !uri.Host.Contains("githubusercontent.com"))
                    return null;
                
                var pathSegments = uri.AbsolutePath.Split('/');
                
                // Expected formats:
                // github.com: /owner/repo/blob/branch/... or /owner/repo/tree/branch/...
                // githubusercontent.com: /owner/repo/branch/...
                
                if (pathSegments.Length < 3)
                    return null;
                
                string owner = pathSegments[1];
                string repo = pathSegments[2];
                string branch = "main"; // Default branch
                
                if (uri.Host.Contains("raw.githubusercontent.com"))
                {
                    // Format: /owner/repo/branch/...
                    if (pathSegments.Length >= 4)
                        branch = pathSegments[3];
                }
                else
                {
                    // Format: /owner/repo/blob/branch/... or /owner/repo/tree/branch/...
                    for (int i = 3; i < pathSegments.Length; i++)
                    {
                        if (pathSegments[i] == "blob" || pathSegments[i] == "tree")
                        {
                            if (i + 1 < pathSegments.Length)
                            {
                                branch = pathSegments[i + 1];
                                break;
                            }
                        }
                    }
                }
                
                return (owner, repo, branch);
            }
            catch
            {
                return null;
            }
        }
    }
}