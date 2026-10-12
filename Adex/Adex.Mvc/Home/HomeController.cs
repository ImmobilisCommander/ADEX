using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adex.Mvc.Hosting;
using Adex.Mvc.Api;
using Adex.Mvc.Entities;
using Adex.Mvc.Home;
using Adex.Mvc.Shared;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Adex.Mvc.Home
{
    public class HomeController(IAdexWebApiClient apiClient) : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Explorer les liens d’intérêt";
            return View(new DashboardViewModel());
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DashboardContent(CancellationToken cancellationToken)
        {
            var model = await apiClient.TryGetDashboardAsync(cancellationToken);
            return model is null
                ? StatusCode(StatusCodes.Status202Accepted)
                : PartialView("_DashboardContent", model.ToViewModel());
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
            query ??= string.Empty;
            ViewData["Title"] = "Résultats de recherche";
            var results = await apiClient.SearchEntitiesAsync(query, cancellationToken);
            List<EntitySearchResultViewModel> viewModels =
                results.Select(r => r.ToViewModel()).ToList();

            if (Request.Headers.Accept.ToString().Contains("application/json", System.StringComparison.OrdinalIgnoreCase))
            {
                return Ok(viewModels);
            }

            return View(
                "Search",
                new EntitySearchPageViewModel { Query = query, Results = viewModels }
            );
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
