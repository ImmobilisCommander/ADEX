using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Api;
using Adex.Mvc.Entities;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Entities
{
    public sealed class EntityController(IAdexWebApiClient apiClient) : Controller
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
            if (
                string.IsNullOrEmpty(fromQuery)
                && Request.Headers.TryGetValue("Referer", out var refererValues)
            )
            {
                var referer = refererValues.ToString();
                if (
                    Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                    && refererUri.AbsolutePath.Equals("/Search", StringComparison.OrdinalIgnoreCase)
                )
                {
                    var parsed = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                        refererUri.Query
                    );
                    if (parsed.TryGetValue("query", out var q))
                    {
                        fromQuery = q.ToString().Trim();
                    }
                }
            }

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
            return View(model);
        }
    }
}
