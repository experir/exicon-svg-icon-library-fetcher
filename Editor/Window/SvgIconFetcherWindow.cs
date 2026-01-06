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

        [MenuItem("Tools/SVG Icon Fetcher")]
        public static void Open()
        {
            GetWindow<SvgIconFetcherWindow>("SVG Icon Fetcher");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("SVG Icon Fetcher", EditorStyles.boldLabel);

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
            EditorGUILayout.LabelField("Icons", EditorStyles.boldLabel);

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
                var url = $"{source.BaseUrl}/{icon}.svg";
                var svg = await SvgDownloader.Download(url);
                svg = SvgSanitizer.Sanitize(svg);

                if (!System.IO.Directory.Exists(outputFolder))
    System.IO.Directory.CreateDirectory(outputFolder);

var path = System.IO.Path.Combine(outputFolder, $"{icon}.svg");
System.IO.File.WriteAllText(path, svg);
AssetDatabase.ImportAsset(path);

            }

            AssetDatabase.Refresh();
        }
    }
}
