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

            if (GUILayout.Button("Download Selected"))
                DownloadSelected();
                
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
            var source = IconSourceRegistry.Sources[selectedSourceIndex];

            foreach (var icon in selectedIcons)
            {
                try
                {
                    var url = $"{source.BaseUrl}/{icon}.svg";
                    Debug.Log($"Attempting to download: {url}");
                    
                    var svg = await SvgDownloader.Download(url);
                    svg = SvgSanitizer.Sanitize(svg);

                    if (!System.IO.Directory.Exists(outputFolder))
                        System.IO.Directory.CreateDirectory(outputFolder);

                    var path = System.IO.Path.Combine(outputFolder, $"{icon}.svg");
                    System.IO.File.WriteAllText(path, svg);
                    AssetDatabase.ImportAsset(path);
                    
                    Debug.Log($"✓ Downloaded: {icon}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"✗ Failed to download '{icon}': {e.Message}");
                }
            }

            AssetDatabase.Refresh();
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
                
                // Download and sanitize SVG
                var svg = await SvgDownloader.Download(rawUrl);
                customIconPreview = SvgSanitizer.Sanitize(svg);
                
                // Save to disk
                if (!System.IO.Directory.Exists(outputFolder))
                    System.IO.Directory.CreateDirectory(outputFolder);
                
                var path = System.IO.Path.Combine(outputFolder, $"{customIconName}.svg");
                System.IO.File.WriteAllText(path, customIconPreview);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
                
                Debug.Log($"Downloaded custom icon: {customIconName}");
                EditorUtility.DisplayDialog("Success", 
                    $"Icon '{customIconName}.svg' downloaded successfully to:\n{outputFolder}", 
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
    }
}
