using Adex.Mvc.Api;
using Adex.WebApi.Client;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Entities
{
    public sealed class EntityController(IAdexWebApiClient apiClient) : Controller
    {
        [HttpGet("/{id:guid}")]
        public async Task<IActionResult> Details(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] string fromQuery = null,
            [FromQuery] int? limit = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            var entity = await apiClient.GetEntityAsync(
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

            var requestedLimit = limit is > 0
                ? limit.Value
                : EntityDetailsViewModel.DefaultPlanetaryLimit;

            var counterparties = await apiClient.GetEntityCounterpartiesAsync(
                id,
                limit: requestedLimit,
                cancellationToken: cancellationToken
            );
            model.BuildPlanetarySystem(counterparties, requestedLimit);

            ViewData["Title"] = model.Name;
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
                : EntityDetailsViewModel.DefaultPlanetaryLimit;

            var counterparties = await apiClient.GetEntityCounterpartiesAsync(
                id,
                limit: requestedLimit,
                cancellationToken: cancellationToken
            );
            if (counterparties is null)
            {
                return NotFound();
            }

            fromQuery = fromQuery?.Trim();
            var viewModel = PlanetarySystemViewModel.Create(
                id,
                counterparties.EntityName,
                counterparties,
                requestedLimit,
                fromQuery
            );

            return PartialView("_PlanetaryViewport", viewModel);
        }
    }
}
