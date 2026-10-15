using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Configuration;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.Mvc.Sitemap
{
    public sealed class SitemapController(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions
    ) : Controller
    {
        private static readonly string[] StaticRelativePaths =
        [
            "/",
            "/Csv",
            "/Home/Settings",
            "/Home/Disclaimer",
            "/Search",
        ];

        [HttpGet]
        [Route("/sitemap.xml")]
        [Route("/sitemap")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var wantsHtml = Request.Path.Value?.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) != true
                && Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);

            var configuredDuration = cacheOptions.CurrentValue.Sitemap;

            if (!wantsHtml && configuredDuration > TimeSpan.Zero)
            {
                var xmlCacheKey = $"sitemap_xml:{baseUrl}";
                if (cache.TryGetValue(xmlCacheKey, out (string Xml, int DurationSeconds) cachedXml))
                {
                    Response.Headers.CacheControl = $"public, max-age={cachedXml.DurationSeconds}";
                    return Content(cachedXml.Xml, "application/xml", Encoding.UTF8);
                }
            }

            const string dataCacheKey = "sitemap_data";
            Contracts.Explorer.SitemapModel sitemapData = null;

            if (configuredDuration > TimeSpan.Zero && cache.TryGetValue(dataCacheKey, out Contracts.Explorer.SitemapModel cachedData))
            {
                sitemapData = cachedData;
            }
            else
            {
                sitemapData = await apiClient.GetSitemapAsync(cancellationToken);
                var effectiveDuration = configuredDuration > TimeSpan.Zero
                    ? configuredDuration
                    : (sitemapData.CacheDuration > TimeSpan.Zero ? sitemapData.CacheDuration : TimeSpan.FromMinutes(30));

                if (configuredDuration > TimeSpan.Zero)
                {
                    cache.Set(dataCacheKey, sitemapData, effectiveDuration);
                }
            }

            var cacheDuration = configuredDuration > TimeSpan.Zero
                ? configuredDuration
                : (sitemapData.CacheDuration > TimeSpan.Zero ? sitemapData.CacheDuration : TimeSpan.FromMinutes(30));

            var cacheDurationSeconds = (int)cacheDuration.TotalSeconds;
            if (cacheDurationSeconds > 0)
            {
                Response.Headers.CacheControl = $"public, max-age={cacheDurationSeconds}";
            }

            if (wantsHtml)
            {
                var staticLinks = new List<SitemapLinkViewModel>
                {
                    new() { Url = $"{baseUrl}/", Title = "Explorer les liens d’intérêt (Accueil)" },
                    new() { Url = $"{baseUrl}/Csv", Title = "Interroger le CSV" },
                    new() { Url = $"{baseUrl}/Home/Settings", Title = "Paramètres" },
                    new() { Url = $"{baseUrl}/Home/Disclaimer", Title = "Source et avertissement" },
                    new() { Url = $"{baseUrl}/Search", Title = "Recherche d’entités" },
                };

                var contributors = sitemapData.Contributors
                    .Select(c => new SitemapLinkViewModel
                    {
                        Url = $"{baseUrl}/{c.Id}",
                        Title = c.Name,
                    })
                    .ToList();

                var beneficiaries = sitemapData.Beneficiaries
                    .Select(b => new SitemapLinkViewModel
                    {
                        Url = $"{baseUrl}/{b.Id}",
                        Title = b.Name,
                    })
                    .ToList();

                var model = new SitemapPageViewModel
                {
                    StaticLinks = staticLinks,
                    TopContributors = contributors,
                    TopBeneficiaries = beneficiaries,
                    XmlSitemapUrl = $"{baseUrl}/sitemap.xml",
                };

                ViewData["Title"] = model.Title;
                return View(model);
            }

            var xml = SitemapXmlBuilder.BuildXml(
                baseUrl,
                StaticRelativePaths,
                sitemapData.Contributors.Select(c => c.Id),
                sitemapData.Beneficiaries.Select(b => b.Id)
            );

            if (cacheDuration > TimeSpan.Zero)
            {
                cache.Set(
                    $"sitemap_xml:{baseUrl}",
                    (Xml: xml, DurationSeconds: cacheDurationSeconds),
                    cacheDuration
                );
            }

            return Content(xml, "application/xml", Encoding.UTF8);
        }

        [HttpGet]
        [Route("/robots.txt")]
        public IActionResult Robots()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var content =
                $"User-agent: *\n"
                + "# Les fiches HTML (dont /{id}) et leurs representations JSON sont explorables.\n"
                + "Content-Signal: search=yes, ai-input=yes, ai-train=no\n"
                + "Allow: /\n"
                + "Allow: /sitemap\n"
                + "Allow: /sitemap.xml\n"
                + "Allow: /data/entities/\n"
                + "# Exclure les routes API, recherche, CSV et utilitaires du crawl.\n"
                + "Disallow: /api/\n"
                + "Disallow: /Search\n"
                + "Disallow: /Csv\n"
                + "Disallow: /Home/Settings\n"
                + "Disallow: /Home/Error\n"
                + "Disallow: /*/planetary\n\n"
                + $"Sitemap: {baseUrl}/sitemap.xml\n";
            return Content(content, "text/plain", Encoding.UTF8);
        }
    }
}
