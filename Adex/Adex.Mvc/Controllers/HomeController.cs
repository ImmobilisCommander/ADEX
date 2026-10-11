using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adex.Mvc.Hosting;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Adex.Mvc.Controllers
{
    public class HomeController(AdexApiClient apiClient) : Controller
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
                : PartialView("_DashboardContent", model);
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
            List<EntitySearchResultViewModel> results =
                await apiClient.SearchEntitiesAsync(query, cancellationToken);

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
            ViewData["Title"] = "Erreur";
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            var message = exception is null
                ? "Une erreur inattendue s’est produite."
                : ExceptionMapping.Map(exception).Message;
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
