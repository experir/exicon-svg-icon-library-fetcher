using System.Collections.Generic;
using SvgIconFetcher.Models;

namespace SvgIconFetcher.Core
{
    public static class IconSourceRegistry
    {
        public static readonly List<IconSource> Sources = new()
        {
            new IconSource
            {
                Name = "Lucide",
                BaseUrl = "https://github.com/lucide-icons/lucide/raw/main/icons",
                IndexUrl = "https://api.github.com/repos/lucide-icons/lucide/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "ISC",
                PathFilter = "icons/"
            },
            new IconSource
            {
                Name = "Tabler Outline",
                BaseUrl = "https://github.com/tabler/tabler-icons/raw/main/icons/outline",
                IndexUrl = "https://api.github.com/repos/tabler/tabler-icons/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/outline/"
            },
            new IconSource
            {
                Name = "Tabler Filled",
                BaseUrl = "https://github.com/tabler/tabler-icons/raw/main/icons/filled",
                IndexUrl = "https://api.github.com/repos/tabler/tabler-icons/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/filled/"
            }
        };
    }
}
