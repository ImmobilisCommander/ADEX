using Adex.Contracts.Explorer;
using Adex.Mvc.Api;
using Adex.Mvc.Configuration;
using Adex.Mvc.Entities.Json;
using Adex.WebApi.Client;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Entities
{
    public sealed class EntityController(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions,
        EntityDetailsCache entityDetailsCache
    ) : Controller
    {
        [HttpGet("/{id:guid}")]
        public async Task<IActionResult> Details(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] string fromQuery = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            var entity = await entityDetailsCache.GetAsync(
                id,
                page,
                pageSize,
                sort ?? "date",
                descending,
                cancellationToken
            );

            if (entity is null)
            {
                ViewData["Title"] = "Entité introuvable";
                Response.StatusCode = 404;
                return View("NotFound", new NotFoundViewModel { Identifier = id.ToString() });
            }

            fromQuery = fromQuery?.Trim();

            var model = entity.ToViewModel();
            if (!string.IsNullOrEmpty(fromQuery))
            {
                model.SearchQuery = fromQuery;
                foreach (var link in model.FinancialLinks)
                {
                    link.SearchQuery = fromQuery;
                }
            }

            ViewData["Title"] = model.Name;
            model.JsonUrl = Url.RouteUrl("entity-json", new { id, page, pageSize, sort, descending });
            ViewData["AlternateJsonUrl"] = model.JsonUrl;
            return View(model);
        }

        [HttpGet("/{id:guid}/planetary")]
        public async Task<IActionResult> Planetary(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int? limit = null,
            [FromQuery] string fromQuery = null
        )
        {
            var requestedLimit = limit is > 0
                ? limit.Value
                : EntityDetailsViewModel.DefaultCounterpartyLimit;

            var counterparties = await GetCachedCounterpartiesAsync(id, requestedLimit, cancellationToken);
            if (counterparties is null)
            {
                return NotFound();
            }

            var viewModel = PlanetarySystemViewModel.Create(
                id,
                counterparties.EntityName,
                counterparties,
                requestedLimit,
                fromQuery?.Trim()
            );

            return PartialView("~/Views/Shared/Components/PlanetarySystem/_Viewport.cshtml", viewModel);
        }

        private async Task<EntityCounterpartiesResult> GetCachedCounterpartiesAsync(
            Guid id,
            int limit,
            CancellationToken cancellationToken
        )
        {
            var duration = cacheOptions.CurrentValue.Counterparties;
            var cacheKey = $"mvc_counterparties:{id}:{limit}";

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityCounterpartiesResult cached))
            {
                return cached;
            }

            var counterparties = await apiClient.GetEntityCounterpartiesAsync(
                id,
                limit: limit,
                cancellationToken: cancellationToken
            );

            if (counterparties is not null && duration > TimeSpan.Zero)
            {
                cache.Set(cacheKey, counterparties, duration);
            }

            return counterparties;
        }
    }
}
