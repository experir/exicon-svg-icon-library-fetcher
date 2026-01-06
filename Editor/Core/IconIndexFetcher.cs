using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using SvgIconFetcher.Models;

namespace SvgIconFetcher.Core
{
    [System.Serializable]
    internal class GitHubContentItem
    {
        public string name;
        public string type;
    }

    [System.Serializable]
    internal class GitHubContentWrapper
    {
        public GitHubContentItem[] items;
    }

    [System.Serializable]
    internal class GitHubTreeItem
    {
        public string path;
        public string type;
    }

    [System.Serializable]
    internal class GitHubTreeResponse
    {
        public GitHubTreeItem[] tree;
        public bool truncated;
    }

    public static class IconIndexFetcher
    {
        public static async Task<List<string>> FetchIcons(IconSource source)
        {
            // 1️⃣ Try cache first (NO API CALLS)
            if (IconIndexCache.TryLoad(source.Name, out var cachedIcons))
            {
                return cachedIcons;
            }

            var results = new List<string>();

            // 2️⃣ Fetch from GitHub using Git Tree API (single request, much faster!)
            using var req = UnityWebRequest.Get(source.IndexUrl);
            req.SetRequestHeader("User-Agent", "UnitySvgIconFetcher");
            req.SetRequestHeader("Accept", "application/vnd.github+json");

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            // Handle GitHub limits clearly
            if (req.responseCode == 403)
            {
                string rateLimitReset = req.GetResponseHeader("X-RateLimit-Reset");
                
                string errorMsg = "GitHub API rate limit reached (60 requests/hour)!\n\n";
                
                if (!string.IsNullOrEmpty(rateLimitReset))
                {
                    long resetTime = long.Parse(rateLimitReset);
                    var resetDate = System.DateTimeOffset.FromUnixTimeSeconds(resetTime).LocalDateTime;
                    errorMsg += $"Rate limit resets at: {resetDate}\n\n";
                }
                
                errorMsg += "Solutions:\n";
                errorMsg += "• Wait for rate limit reset\n";
                errorMsg += "• Use cached data from previous loads";
                
                throw new System.Exception(errorMsg);
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new System.Exception(req.error);
            }

            results = ParseGitHubTreeJson(req.downloadHandler.text, source);

            // 4️⃣ Save cache for next runs
            IconIndexCache.Save(source.Name, results);

            return results;
        }

        private static List<string> ParseGitHubTreeJson(string json, IconSource source)
        {
            var icons = new List<string>();
            var response = UnityEngine.JsonUtility.FromJson<GitHubTreeResponse>(json);

            if (response?.tree == null)
                return icons;

            foreach (var item in response.tree)
            {
                if (item.type == "blob" && item.path.EndsWith(".svg"))
                {
                    // Filter by path if specified
                    if (!string.IsNullOrEmpty(source.PathFilter))
                    {
                        if (!item.path.StartsWith(source.PathFilter))
                            continue;
                    }
                    
                    // Get relative path after removing PathFilter
                    var relativePath = item.path;
                    if (!string.IsNullOrEmpty(source.PathFilter))
                    {
                        relativePath = relativePath.Substring(source.PathFilter.Length);
                    }
                    
                    // Skip files in subfolders (only include root level icons)
                    // e.g., skip "test/file.svg", only include "file.svg"
                    if (relativePath.Contains("/"))
                        continue;
                    
                    // Extract just the filename without extension
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(relativePath);
                    icons.Add(fileName);
                }
            }

            return icons;
        }

        private static List<string> ParseGitHubJson(string json)
        {
            var icons = new List<string>();

            // JsonUtility cannot parse root arrays → wrap
            json = "{\"items\":" + json + "}";

            var wrapper = UnityEngine.JsonUtility.FromJson<GitHubContentWrapper>(json);

            if (wrapper?.items == null)
                return icons;

            foreach (var item in wrapper.items)
            {
                if (item.type == "file" && item.name.EndsWith(".svg"))
                {
                    icons.Add(item.name.Replace(".svg", ""));
                }
            }

            return icons;
        }
    }
}
