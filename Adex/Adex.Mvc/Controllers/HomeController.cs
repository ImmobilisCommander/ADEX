using Adex.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Adex.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Search/{txt}")]
        public async Task<ActionResult> Search(string txt)
        {
            _stopwatch.Restart();
            Dictionary<string, string> data = null;

            using (var c = new WebClient())
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, string>>(await c.DownloadStringTaskAsync($"https://localhost:44329/api/search/{txt}"));
            }

            _stopwatch.Stop();
            data.Add("Elapsed time", $"{_stopwatch.Elapsed.TotalMilliseconds} ms");

            return new JsonResult(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
