using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Adex.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly AdexApiClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AdexApiClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                return View(await _apiClient.GetDashboardAsync(cancellationToken));
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Unable to load dashboard data from the API");
                return View(
                    new DashboardViewModel
                    {
                        ErrorMessage =
                            "Les données ne sont pas accessibles. Vérifiez que l’API et la base sont disponibles."
                    }
                );
            }
        }

        [HttpGet]
        [Route("/Search")]
        public async Task<ActionResult> Search(
            [FromQuery] string query,
            CancellationToken cancellationToken
        )
        {
            query ??= string.Empty;
            List<EntitySearchResultViewModel> results;
            try
            {
                results = await _apiClient.SearchEntitiesAsync(query, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Entity search failed for query {Query}", query);
                if (Request.Headers.Accept.ToString().Contains("application/json", System.StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status502BadGateway,
                        new { message = "La recherche est indisponible." }
                    );
                }

                return View(
                    "Search",
                    new EntitySearchPageViewModel
                    {
                        Query = query,
                        ErrorMessage = "La recherche est indisponible. Réessayez dans un instant."
                    }
                );
            }

            if (Request.Headers.Accept.ToString().Contains("application/json", System.StringComparison.OrdinalIgnoreCase))
            {
                return new JsonResult(results);
            }

            return View(
                "Search",
                new EntitySearchPageViewModel { Query = query, Results = results }
            );
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
