using System.Text.RegularExpressions;

namespace SvgIconFetcher.Core
{
    public static class SvgSanitizer
    {
        public static string Sanitize(string svg)
        {
            if (string.IsNullOrEmpty(svg))
                return svg;

            svg = Regex.Replace(svg, @"currentColor", "#000000", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"style\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"stroke-linecap\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"stroke-linejoin\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"crossorigin(\s*=\s*""[^""]*"")?", "", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"<!--.*?-->", "", RegexOptions.Singleline);

            return svg;
        }
    }
}
