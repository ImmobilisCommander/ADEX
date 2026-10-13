using System;
using System.Collections.Generic;

namespace Adex.Contracts.Explorer
{
    public sealed class SitemapModel
    {
        public List<SitemapEntityItem> Contributors { get; set; } = new();

        public List<SitemapEntityItem> Beneficiaries { get; set; } = new();

        public TimeSpan CacheDuration { get; set; }
    }
}
