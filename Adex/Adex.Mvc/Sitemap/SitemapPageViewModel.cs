using System.Collections.Generic;

namespace Adex.Mvc.Sitemap
{
    public sealed class SitemapPageViewModel
    {
        public string Title => "Plan du site";

        public IReadOnlyList<SitemapLinkViewModel> StaticLinks { get; init; } = [];

        public IReadOnlyList<SitemapLinkViewModel> TopContributors { get; init; } = [];

        public IReadOnlyList<SitemapLinkViewModel> TopBeneficiaries { get; init; } = [];

        public string XmlSitemapUrl { get; init; } = "/sitemap.xml";
    }
}
