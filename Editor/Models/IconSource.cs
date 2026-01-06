namespace SvgIconFetcher.Models
{
    public enum IconIndexType
    {
        GitHubApi,
        RawJson,
        CustomParser
    }

    public class IconSource
    {
        public string Name;
        public string BaseUrl;
        public string IndexUrl;
        public IconIndexType IndexType;
        public string License;
        public string PathFilter;
    }
}
