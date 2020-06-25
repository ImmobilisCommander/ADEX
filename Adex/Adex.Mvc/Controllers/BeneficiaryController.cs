using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Adex.Mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController : ControllerBase
    {
        private readonly ILogger<BeneficiaryController> _logger;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        [HttpGet]
        [Route("{id}")]
        [Route("Read/{id}")]
        public async Task<ActionResult> Read(string id)
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
    }
}
