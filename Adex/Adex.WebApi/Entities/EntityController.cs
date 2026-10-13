using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.Application.Configuration;
using Adex.Contracts.Explorer;
using Adex.Data.ReadModel.Explorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.WebApi.Entities
{
    /// <summary>
    /// Contrôleur de recherche et de consultation détaillée des entités (entreprises et bénéficiaires).
    /// </summary>
    [ApiController]
    [Route("api/entity")]
    public sealed class EntityController(
        DataExplorerService explorer,
        IMemoryCache cache,
        IOptionsMonitor<CacheOptions> cacheOptions
    ) : ControllerBase
    {
        /// <summary>
        /// Recherche les entités (entreprises ou bénéficiaires) par mots-clés sur l'identité ou la dénomination.
        /// </summary>
        /// <param name="query">Terme textuel recherché (nom, prénom ou raison sociale).</param>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Liste des résultats correspondants avec montants globaux et typologies.</returns>
        /// <response code="200">Liste des entités trouvées (plafonnée à 100 résultats).</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<EntitySearchResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EntitySearchResult>>> Search(
            [FromQuery] string query,
            CancellationToken cancellationToken
        )
        {
            var trimmed = query?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmed))
            {
                return new List<EntitySearchResult>();
            }

            var duration = cacheOptions.CurrentValue.Search;
            if (duration <= TimeSpan.Zero)
            {
                return await explorer.SearchEntitiesAsync(trimmed, cancellationToken);
            }

            var cacheKey = $"entity_search:{trimmed.ToLowerInvariant()}";
            if (cache.TryGetValue(cacheKey, out List<EntitySearchResult> cached))
            {
                return cached;
            }

            var results = await explorer.SearchEntitiesAsync(trimmed, cancellationToken);
            cache.Set(cacheKey, results, duration);
            return results;
        }

        /// <summary>
        /// Récupère la fiche complète d'une entité et la liste paginée de ses liens financiers.
        /// </summary>
        /// <param name="id">Identifiant unique (GUID) de l'entité.</param>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <param name="page">Numéro de page pour la liste des liens d'intérêt (défaut : 1).</param>
        /// <param name="pageSize">Nombre d'éléments par page (défaut : 20).</param>
        /// <param name="sort">Propriété de tri des liens ("date", "amount", etc., défaut : "date").</param>
        /// <param name="descending">Indique si le tri doit être décroissant (défaut : true).</param>
        /// <returns>Fiche détaillée avec répartition annuelle, répartition par typologie et liens financiers.</returns>
        /// <response code="200">Entité trouvée et retournée.</response>
        /// <response code="404">Aucune entité trouvée pour cet identifiant.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EntityDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EntityDetails>> Get(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DataExplorerService.DefaultPageSize,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            var duration = cacheOptions.CurrentValue.Entity;
            var cacheKey = $"api_entity:{id}:{page}:{pageSize}:{sort}:{descending}";

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityDetails cached))
            {
                return Ok(cached);
            }

            var entity = await explorer.GetEntityAsync(
                id,
                page,
                pageSize,
                sort,
                descending,
                cancellationToken
            );

            if (entity is null)
            {
                return NotFound();
            }

            if (duration > TimeSpan.Zero)
            {
                cache.Set(cacheKey, entity, duration);
            }

            return Ok(entity);
        }

        /// <summary>
        /// Récupère la liste des contreparties d'une entité regroupées avec la somme des montants.
        /// </summary>
        /// <param name="id">Identifiant unique (GUID) de l'entité.</param>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <param name="limit">Nombre maximal de contreparties à retourner (optionnel, classées par montant décroissant).</param>
        /// <returns>Résultat contenant le nombre total de contreparties et la liste des contreparties avec montants totaux, entrants, sortants et nombre de liens.</returns>
        /// <response code="200">Résultat des contreparties de l'entité retourné.</response>
        /// <response code="404">Aucune entité trouvée pour cet identifiant.</response>
        [HttpGet("{id:guid}/counterparties")]
        [ProducesResponseType(typeof(EntityCounterpartiesResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EntityCounterpartiesResult>> GetCounterparties(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int? limit = null
        )
        {
            var duration = cacheOptions.CurrentValue.Counterparties;
            var cacheKey = $"api_counterparties:{id}:{limit}";

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityCounterpartiesResult cached))
            {
                return Ok(cached);
            }

            var counterparties = await explorer.GetEntityCounterpartiesAsync(
                id,
                limit,
                cancellationToken
            );

            if (counterparties is null)
            {
                return NotFound();
            }

            if (duration > TimeSpan.Zero)
            {
                cache.Set(cacheKey, counterparties, duration);
            }

            return Ok(counterparties);
        }
    }
}
