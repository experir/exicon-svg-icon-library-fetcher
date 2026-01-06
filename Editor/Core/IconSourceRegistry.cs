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
            },
            new IconSource
            {
                Name = "Heroicons Outline",
                BaseUrl = "https://github.com/tailwindlabs/heroicons/raw/master/optimized/24/outline",
                IndexUrl = "https://api.github.com/repos/tailwindlabs/heroicons/git/trees/master?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "optimized/24/outline/"
            },
            new IconSource
            {
                Name = "Heroicons Solid",
                BaseUrl = "https://github.com/tailwindlabs/heroicons/raw/master/optimized/24/solid",
                IndexUrl = "https://api.github.com/repos/tailwindlabs/heroicons/git/trees/master?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "optimized/24/solid/"
            },
            new IconSource
            {
                Name = "Bootstrap Icons",
                BaseUrl = "https://github.com/twbs/icons/raw/main/icons",
                IndexUrl = "https://api.github.com/repos/twbs/icons/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/"
            },
            new IconSource
            {
                Name = "Feather Icons",
                BaseUrl = "https://github.com/feathericons/feather/raw/main/icons",
                IndexUrl = "https://api.github.com/repos/feathericons/feather/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/"
            },
            new IconSource
            {
                Name = "Ionicons Outline",
                BaseUrl = "https://github.com/ionic-team/ionicons/raw/main/src/svg",
                IndexUrl = "https://api.github.com/repos/ionic-team/ionicons/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "src/svg/"
            },
            new IconSource
            {
                Name = "Phosphor Icons",
                BaseUrl = "https://github.com/phosphor-icons/core/raw/main/assets/regular",
                IndexUrl = "https://api.github.com/repos/phosphor-icons/core/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "assets/regular/"
            },
            new IconSource
            {
                Name = "Octicons",
                BaseUrl = "https://github.com/primer/octicons/raw/main/icons",
                IndexUrl = "https://api.github.com/repos/primer/octicons/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/"
            },
            new IconSource
            {
                Name = "Iconoir",
                BaseUrl = "https://github.com/iconoir-icons/iconoir/raw/main/icons/regular",
                IndexUrl = "https://api.github.com/repos/iconoir-icons/iconoir/git/trees/main?recursive=1",
                IndexType = IconIndexType.GitHubApi,
                License = "MIT",
                PathFilter = "icons/regular/"
            }
        };
    }
}
