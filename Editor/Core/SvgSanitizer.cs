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
            svg = Regex.Replace(svg, @"crossorigin(\s*=\s*""[^""]*"")?", "", RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"<!--.*?-->", "", RegexOptions.Singleline);

            return svg;
        }

        /// <summary>
        /// Replaces all fill and stroke colors in the SVG with the given hex color.
        /// Also replaces currentColor, #000000, #000, black, and inline style colors.
        /// </summary>
        public static string OverrideColor(string svg, string hexColor)
        {
            if (string.IsNullOrEmpty(svg) || string.IsNullOrEmpty(hexColor))
                return svg;

            // Normalize hex to #RRGGBB
            if (!hexColor.StartsWith("#"))
                hexColor = "#" + hexColor;

            // Replace fill="..." and stroke="..." attribute values (skip "none")
            svg = Regex.Replace(svg,
                @"(fill\s*=\s*"")((?!none)[^""]*)("")",
                $"$1{hexColor}$3", RegexOptions.IgnoreCase);

            svg = Regex.Replace(svg,
                @"(stroke\s*=\s*"")((?!none)[^""]*)("")",
                $"$1{hexColor}$3", RegexOptions.IgnoreCase);

            // Replace common color keywords and hex values in the whole SVG
            svg = Regex.Replace(svg, @"currentColor", hexColor, RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"#000000", hexColor, RegexOptions.IgnoreCase);
            svg = Regex.Replace(svg, @"#000(?![0-9a-fA-F])", hexColor, RegexOptions.IgnoreCase);

            // Replace color values inside inline style attributes
            svg = Regex.Replace(svg,
                @"((?:fill|stroke)\s*:\s*)(?!none)#?[0-9a-fA-F]{3,6}",
                $"$1{hexColor}", RegexOptions.IgnoreCase);

            return svg;
        }
    }
}
