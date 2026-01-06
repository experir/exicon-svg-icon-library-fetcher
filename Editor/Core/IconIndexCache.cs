using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SvgIconFetcher.Core
{
    public static class IconIndexCache
    {
        private static string CacheDir =>
            Path.Combine(Application.dataPath, "../Library/SvgIconFetcher");

        public static string GetCachePath(string sourceName)
        {
            return Path.Combine(CacheDir, $"icons_{sourceName}.json");
        }

        public static bool TryLoad(string sourceName, out List<string> icons)
        {
            var path = GetCachePath(sourceName);
            icons = null;

            if (!File.Exists(path))
                return false;

            var json = File.ReadAllText(path);
            icons = JsonUtility.FromJson<IconListWrapper>(json)?.icons;
            return icons != null;
        }

        public static void Save(string sourceName, List<string> icons)
        {
            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);

            var json = JsonUtility.ToJson(new IconListWrapper { icons = icons });
            File.WriteAllText(GetCachePath(sourceName), json);
        }

        [System.Serializable]
        private class IconListWrapper
        {
            public List<string> icons;
        }
    }
}
