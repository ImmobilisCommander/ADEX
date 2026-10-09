using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Adex.Mvc.Controllers
{
    public class CsvController : Controller
    {
        private const int PageSize = 10;

        private readonly AdexApiClient _apiClient;
        private readonly ILogger<CsvController> _logger;

        public CsvController(AdexApiClient apiClient, ILogger<CsvController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(
            int page = 1,
            [FromQuery(Name = "filters")] Dictionary<string, string> filters = null
        )
        {
            var model = new CsvIndexViewModel
            {
                PageSize = PageSize,
                ContentUrl = ActionUrl(nameof(Content), Math.Max(page, 1), ActiveFilters(filters)),
            };
            ViewData["Title"] = model.Title;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Content(
            int page = 1,
            [FromQuery(Name = "filters")] Dictionary<string, string> filters = null,
            CancellationToken cancellationToken = default
        )
        {
            var model = await BuildAsync(Math.Max(page, 1), ActiveFilters(filters), cancellationToken);
            return PartialView("_CsvContent", model);
        }

        private static Dictionary<string, string> ActiveFilters(Dictionary<string, string> filters)
        {
            return (filters ?? new Dictionary<string, string>())
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Key, x => x.Value.Trim());
        }

        private async Task<CsvPageViewModel> BuildAsync(
            int page,
            Dictionary<string, string> filters,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var csv = await _apiClient.GetCsvPageAsync(page, filters, cancellationToken);
                if (csv.ErrorMessage is not null)
                {
                    return new CsvPageViewModel { ErrorMessage = csv.ErrorMessage };
                }

                var filterable = csv.FilterableColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
                return new CsvPageViewModel
                {
                    Page = page,
                    PageSize = PageSize,
                    Columns = csv.Columns
                        .Select(name => new CsvColumnViewModel
                        {
                            Name = name,
                            IsFilterable = filterable.Contains(name),
                            FilterValue = filters.GetValueOrDefault(name, string.Empty),
                        })
                        .ToList(),
                    Rows = csv.Rows,
                    PreviousPageUrl = page > 1 ? ActionUrl(nameof(Index), page - 1, filters) : null,
                    NextPageUrl = csv.HasNextPage ? ActionUrl(nameof(Index), page + 1, filters) : null,
                    ClearFiltersUrl = filters.Count > 0 ? Url.Action(nameof(Index)) : null,
                };
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to load CSV data from the API");
                return new CsvPageViewModel
                {
                    ErrorMessage = exception.StatusCode == HttpStatusCode.ServiceUnavailable
                        ? "Un import est en cours, les données sont temporairement indisponibles."
                        : "Les données ne sont pas accessibles. Vérifiez que l’API est disponible."
                };
            }
        }

        private string ActionUrl(string action, int page, Dictionary<string, string> filters)
        {
            var values = new RouteValueDictionary { ["page"] = page };
            foreach (var (column, value) in filters)
            {
                values[$"filters[{column}]"] = value;
            }

            return Url.Action(action, values);
        }
    }
}
