using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adex.Library;
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
        public void Generate(int nbRecords)
        {
            CsvLoader.ReWriteToUTF8(nbRecords);
        }

        [HttpGet]
        [Route("search/{txt}")]
        public IEnumerable<string> Search(string txt)
        {
            return new string []{ "hello", "world" };        
        }
    }
}