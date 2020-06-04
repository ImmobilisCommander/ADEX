using System;
using System.Collections.Generic;
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

        [HttpGet]
        [Route("generate/{nbRecords}")]
        public async Task Generate(int nbRecords)
        {
            var files = Directory.GetFiles(@"E:\Git\ImmobilisCommander\ADEX\exports-etalab", "*.csv");
            foreach (var f in files)
            {
                FileHelper.ReWriteToUTF8(f, @"E:\Git\ImmobilisCommander\ADEX\Data", nbRecords);
            }
        }

        private class MonObjet
        {
            public string Name { get; set; }

            public string[] Tab { get; set; }
        }

        [HttpGet]
        [Route("search/{txt}")]
        public async Task<ActionResult> Search(string txt)
        {
            var obj = new MonObjet { Name = txt, Tab = new string[] { "Hello", "World", txt } };
            return new JsonResult(obj);        
        }
    }
}