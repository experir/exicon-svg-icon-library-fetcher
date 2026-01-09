using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SvgIconFetcher.Core
{
    /// <summary>
    /// Fetches list of SVG files from a GitHub folder URL
    /// </summary>
    public static class FolderIndexFetcher
    {
        /// <summary>
        /// Extract owner and repo from a GitHub folder URL
        /// </summary>
        /// <param name="folderUrl">GitHub URL like: https://github.com/tabler/tabler-icons/tree/main/icons/filled</param>
        /// <returns>Tuple of (owner, repo, branch, folderPath) or null if invalid</returns>
        public static (string owner, string repo, string branch, string folderPath)? ParseGitHubFolderUrl(string folderUrl)
        {
            if (string.IsNullOrEmpty(folderUrl))
                return null;

            try
            {
                var uri = new System.Uri(folderUrl);
                if (!uri.Host.Contains("github.com"))
                    return null;

                var pathSegments = uri.AbsolutePath.Split('/');
                
                // Expected format: /owner/repo/tree/branch/folder/path
                // Or: /owner/repo/tree/branch (just the tree root)
                if (pathSegments.Length < 4)
                    return null;

                string owner = pathSegments[1];
                string repo = pathSegments[2];
                
                // Find "tree" keyword
                int treeIndex = -1;
                for (int i = 3; i < pathSegments.Length; i++)
                {
                    if (pathSegments[i] == "tree")
                    {
                        treeIndex = i;
                        break;
                    }
                }

                if (treeIndex == -1 || treeIndex + 1 >= pathSegments.Length)
                    return null;

                string branch = pathSegments[treeIndex + 1];
                
                // Everything after branch is the folder path
                string folderPath = "";
                if (treeIndex + 2 < pathSegments.Length)
                {
                    folderPath = string.Join("/", pathSegments, treeIndex + 2, pathSegments.Length - treeIndex - 2);
                }

                return (owner, repo, branch, folderPath);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Fetch all SVG files from a GitHub folder
        /// </summary>
        /// <param name="folderUrl">GitHub folder URL</param>
        /// <returns>List of SVG filenames (without extension)</returns>
        public static async Task<List<string>> FetchSvgsFromFolder(string folderUrl)
        {
            var icons = new List<string>();

            var parsed = ParseGitHubFolderUrl(folderUrl);
            if (!parsed.HasValue)
                throw new System.Exception("Invalid GitHub folder URL format.\nExpected: https://github.com/owner/repo/tree/branch/folder/path");

            var (owner, repo, branch, folderPath) = parsed.Value;

            // Build GitHub API URL for getting tree content
            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{branch}?recursive=1";

            using var req = UnityWebRequest.Get(apiUrl);
            req.SetRequestHeader("User-Agent", "UnitySvgIconFetcher");
            req.SetRequestHeader("Accept", "application/vnd.github+json");

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            // Handle GitHub rate limit
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
                throw new System.Exception($"Failed to fetch folder from GitHub: {req.error}");

            // Parse response
            icons = ParseFolderTreeJson(req.downloadHandler.text, folderPath);

            return icons;
        }

        private static List<string> ParseFolderTreeJson(string json, string targetFolder)
        {
            var icons = new List<string>();
            var response = UnityEngine.JsonUtility.FromJson<GitHubTreeResponse>(json);

            if (response?.tree == null)
                return icons;

            // Normalize folder path for comparison
            if (!string.IsNullOrEmpty(targetFolder) && !targetFolder.EndsWith("/"))
                targetFolder += "/";

            foreach (var item in response.tree)
            {
                if (item.type == "blob" && item.path.EndsWith(".svg"))
                {
                    // Check if file is in target folder
                    if (!string.IsNullOrEmpty(targetFolder))
                    {
                        if (!item.path.StartsWith(targetFolder))
                            continue;
                    }

                    // Get path relative to target folder
                    string relativePath = item.path;
                    if (!string.IsNullOrEmpty(targetFolder))
                    {
                        relativePath = relativePath.Substring(targetFolder.Length);
                    }

                    // Only include files directly in the folder, not in subfolders
                    if (relativePath.Contains("/"))
                        continue;

                    // Extract filename without extension
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(relativePath);
                    icons.Add(fileName);
                }
            }

            return icons;
        }
    }
}
