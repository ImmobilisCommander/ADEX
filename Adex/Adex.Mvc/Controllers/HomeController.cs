using System.Diagnostics;
using System.Threading.Tasks;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Adex.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly AdexApiClient _apiClient;

        public HomeController(AdexApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Search/{txt}")]
        public async Task<ActionResult> Search(string txt, CancellationToken cancellationToken)
        {
            var data = await _apiClient.SearchLinksAsync(txt, cancellationToken);
            return new JsonResult(data);
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
