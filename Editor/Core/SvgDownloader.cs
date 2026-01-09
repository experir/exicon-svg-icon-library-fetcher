using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SvgIconFetcher.Core
{
    public static class SvgDownloader
    {
        public static async Task<string> Download(string url)
        {
            using var req = UnityWebRequest.Get(url);
            req.SetRequestHeader("User-Agent", "UnitySvgIconFetcher");

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                throw new System.Exception(req.error);

            return req.downloadHandler.text;
        }

        /// <summary>
        /// Try to download a license file from a GitHub repository
        /// </summary>
        /// <param name="owner">GitHub owner/organization</param>
        /// <param name="repo">GitHub repository name</param>
        /// <param name="branch">Git branch name (usually 'main' or 'master')</param>
        /// <returns>License file content, or null if not found</returns>
        public static async Task<string> TryDownloadLicense(string owner, string repo, string branch)
        {
            // List of common license file names to check
            var licenseFileNames = new List<string>
            {
                "LICENSE",
                "LICENSE.md",
                "LICENSE.txt",
                "LICENCE",
                "LICENCE.md",
                "LICENCE.txt",
                "license",
                "license.md",
                "license.txt",
                "licence",
                "licence.md",
                "licence.txt"
            };

            foreach (var fileName in licenseFileNames)
            {
                try
                {
                    string url = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{fileName}";
                    using var req = UnityWebRequest.Get(url);
                    req.SetRequestHeader("User-Agent", "UnitySvgIconFetcher");

                    var op = req.SendWebRequest();
                    while (!op.isDone)
                        await Task.Yield();

                    // If found and successful, return the content
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        return req.downloadHandler.text;
                    }
                }
                catch
                {
                    // Continue to next filename if this one fails
                    continue;
                }
            }

            // No license file found
            return null;
        }
    }
}
