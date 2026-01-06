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
    }
}
