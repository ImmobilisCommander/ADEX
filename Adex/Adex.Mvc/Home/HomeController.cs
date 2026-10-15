using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.Mvc.Api;
using Adex.Mvc.Configuration;
using Adex.Mvc.Discovery;
using Adex.Mvc.Entities;
using Adex.Mvc.Home;
using Adex.Mvc.Hosting;
using Adex.Mvc.Shared;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.Mvc.Home
{
    public class HomeController(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions
    ) : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Explorer les liens d’intérêt";
            Response.Headers.Append(
                "Link",
                new[]
                {
                    $"<{ApiCatalogController.ApiCatalogPath}>; rel=\"api-catalog\"",
                    $"<{ApiCatalogController.ServiceDocPath}>; rel=\"service-doc\"; type=\"text/html\"",
                    "</sitemap.xml>; rel=\"describedby\"; type=\"application/xml\"",
                }
            );
            return View(new DashboardViewModel());
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DashboardContent(CancellationToken cancellationToken)
        {
            var duration = cacheOptions.CurrentValue.Dashboard;
            const string cacheKey = "mvc_dashboard_content";

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out DashboardViewModel cached))
            {
                return PartialView("_DashboardContent", cached);
            }

            var model = await apiClient.TryGetDashboardAsync(cancellationToken);
            if (model is null)
            {
                return StatusCode(StatusCodes.Status202Accepted);
            }

            var viewModel = model.ToViewModel();
            if (duration > TimeSpan.Zero)
            {
                cache.Set(cacheKey, viewModel, duration);
            }

            return PartialView("_DashboardContent", viewModel);
        }

        [HttpGet]
        public IActionResult Settings()
        {
            var model = new SettingsPageViewModel();
            ViewData["Title"] = model.Title;
            ViewData["SettingsAriaCurrent"] = "page";
            return View(model);
        }

        [HttpGet]
        public IActionResult Disclaimer()
        {
            var model = new DisclaimerPageViewModel();
            ViewData["Title"] = model.Title;
            return View(model);
        }

        [HttpGet]
        [Route("/Search")]
        public async Task<ActionResult> Search(
            [FromQuery] string query,
            CancellationToken cancellationToken
        )
        {
            query = query?.Trim() ?? string.Empty;
            List<EntitySearchResultViewModel> viewModels = new();

            if (!string.IsNullOrEmpty(query))
            {
                var duration = cacheOptions.CurrentValue.Search;
                var cacheKey = $"mvc_search:{query.ToLowerInvariant()}";

                if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out List<EntitySearchResultViewModel> cached))
                {
                    viewModels = cached;
                }
                else
                {
                    var results = await apiClient.SearchEntitiesAsync(query, cancellationToken);
                    viewModels = results
                        .Select(r => r.ToViewModel())
                        .ToList();

                    if (duration > TimeSpan.Zero)
                    {
                        cache.Set(cacheKey, viewModels, duration);
                    }
                }
            }

            if (
                Request
                    .Headers.Accept.ToString()
                    .Contains("application/json", System.StringComparison.OrdinalIgnoreCase)
            )
            {
                return Ok(viewModels);
            }

            var model = new EntitySearchPageViewModel { Query = query, Results = viewModels };
            ViewData["Title"] = model.PageTitle;

            return View("Search", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["Title"] = "Erreur";
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            var message = exception is null
                ? "Une erreur inattendue s’est produite."
                : exception.Map().Message;
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = message,
                }
            );
        }
    }
}
