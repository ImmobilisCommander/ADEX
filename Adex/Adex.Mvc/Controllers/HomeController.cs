using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Cors;
using System.Net;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

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

        public async Task<ActionResult> Information(string id)
        {
            _stopwatch.Restart();
            Dictionary<string, string> data = null;

            using (var c = new WebClient())
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, string>>(await c.DownloadStringTaskAsync($"https://localhost:44329/api/beneficiary/info/{id}"));
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
