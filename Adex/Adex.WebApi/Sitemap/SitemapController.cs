using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Application.Configuration;
using Adex.Contracts.Explorer;
using Adex.Data.ReadModel.Explorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.WebApi.Sitemap
{
    /// <summary>
    /// Contrôleur fournissant les données pour le plan du site.
    /// </summary>
    [ApiController]
    [Route("api/sitemap")]
    public sealed class SitemapController(
        DataExplorerService explorer,
        IMemoryCache cache,
        IOptionsMonitor<CacheOptions> cacheOptions
    ) : ControllerBase
    {
        private const string CacheKey = "api_sitemap";

        /// <summary>
        /// Récupère les données du plan du site incluant les 10 000 plus gros contributeurs et bénéficiaires.
        /// </summary>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Le modèle du plan du site avec les entités et la durée de cache.</returns>
        /// <response code="200">Données du plan du site retournées avec succès.</response>
        [HttpGet]
        [ProducesResponseType(typeof(SitemapModel), StatusCodes.Status200OK)]
        public async Task<ActionResult<SitemapModel>> Get(CancellationToken cancellationToken)
        {
            var duration = cacheOptions.CurrentValue.Sitemap;
            if (duration > TimeSpan.Zero && cache.TryGetValue(CacheKey, out SitemapModel cached))
            {
                return cached;
            }

            var sitemap = await explorer.GetSitemapAsync(cancellationToken);
            if (sitemap is not null && duration > TimeSpan.Zero)
            {
                sitemap.CacheDuration = duration;
                cache.Set(CacheKey, sitemap, duration);
            }

            return sitemap;
        }
    }
}
