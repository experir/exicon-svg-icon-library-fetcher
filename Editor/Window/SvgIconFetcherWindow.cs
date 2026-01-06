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

        [MenuItem("Tools/SVG Icon Fetcher")]
        public static void Open()
        {
            GetWindow<SvgIconFetcherWindow>("SVG Icon Fetcher");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("SVG Icon Fetcher", EditorStyles.boldLabel);
            
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);
            
            // Select All / Deselect All toggle
            if (icons.Count > 0)
            {
                bool allSelected = selectedIcons.Count == icons.Count;
                bool newAllSelected = EditorGUILayout.ToggleLeft("Select All", allSelected, GUILayout.Width(100));
                
                if (newAllSelected != allSelected)
                {
                    if (newAllSelected)
                    {
                        // Select all
                        selectedIcons.Clear();
                        foreach (var icon in icons)
                            selectedIcons.Add(icon);
                    }
                    else
                    {
                        // Deselect all
                        selectedIcons.Clear();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Warning about download time
            if (icons.Count > 0)
            {
                EditorGUILayout.HelpBox("Downloading all icons may take some time. Consider selecting only the ones you need.", MessageType.Info);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
            foreach (var icon in icons)
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
                
            // Method 2: Custom URL
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Method 2: Download from Custom URL", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "Paste a direct link to an SVG icon from GitHub.\n" +
                "Example: https://github.com/lucide-icons/lucide/blob/main/icons/heart.svg",
                MessageType.Info
            );
            
            customUrl = EditorGUILayout.TextField("Icon URL", customUrl);
            
            EditorGUI.BeginDisabledGroup(isLoadingCustom || string.IsNullOrEmpty(customUrl));
            if (GUILayout.Button(isLoadingCustom ? "Downloading..." : "Download Icon"))
                DownloadCustomIcon();
            EditorGUI.EndDisabledGroup();
            
            // Show preview after download
            if (!string.IsNullOrEmpty(customIconPreview))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Preview: {customIconName}", EditorStyles.boldLabel);
                
                // Show SVG code preview in a scrollable text area
                EditorGUILayout.LabelField("SVG Code:", EditorStyles.miniLabel);
                customPreviewScroll = EditorGUILayout.BeginScrollView(customPreviewScroll, GUILayout.Height(150));
                EditorGUILayout.TextArea(customIconPreview, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.HelpBox("Icon loaded successfully! Click 'Download Icon' to save it.", MessageType.Info);
            }
        }

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
                EditorUtility.DisplayDialog("Error Loading Icons", e.Message, "OK");
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
                EditorUtility.DisplayDialog("Cache Cleared", $"Cache cleared for {source.Name}\\nYou can now reload icons.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Cache", "No cache found for this icon pack.", "OK");
            }
        }

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
                
                downloadTask.ContinueWith(task =>
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

                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                var path = System.IO.Path.Combine(folder, $"{icon}.svg");
                System.IO.File.WriteAllText(path, svg);
                AssetDatabase.ImportAsset(path);
                
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
                
                // Save to disk
                if (!System.IO.Directory.Exists(customOutputFolder))
                    System.IO.Directory.CreateDirectory(customOutputFolder);
                
                var path = System.IO.Path.Combine(customOutputFolder, $"{customIconName}.svg");
                System.IO.File.WriteAllText(path, customIconPreview);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
                
                Debug.Log($"Downloaded custom icon: {customIconName} to {customOutputFolder}");
                EditorUtility.DisplayDialog("Success", 
                    $"Icon '{customIconName}.svg' downloaded successfully to:\n{customOutputFolder}", 
                    "OK");
                
                // Clear after download
                customUrl = "";
                customIconPreview = "";
                customIconName = "";
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to download icon:\n{e.Message}\n\n" +
                    "Make sure the URL is a direct link to an SVG file on GitHub.", 
                    "OK");
            }
            finally
            {
                isLoadingCustom = false;
                Repaint();
            }
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
    }
}
