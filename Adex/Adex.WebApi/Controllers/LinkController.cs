using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adex.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        private readonly ILogger<LinkController> _logger;

        public LinkController(ILogger<LinkController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method enable user to search for codes of companies or beneficiaries.
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/{txt}")]
        public async Task<ActionResult> Search(string txt)
        {
            using (var loader = new CsvLoaderNormalized())
            {
                loader.OnMessage += Loader_OnMessage;

                return new JsonResult(loader.LinksToJson(txt, 1000));
            }
        }

        private void Loader_OnMessage(object sender, Common.MessageEventArgs e)
        {
            Debug.Write(e.Message);
        }
    }
}