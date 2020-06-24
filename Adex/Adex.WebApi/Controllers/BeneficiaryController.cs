using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class BeneficiaryController : ControllerBase
    {
        private readonly ILogger<BeneficiaryController> _logger;

        public BeneficiaryController(ILogger<BeneficiaryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This method enable user to search for codes of companies or beneficiaries.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("info/{reference}")]
        public async Task<ActionResult> GetInformation(string reference)
        {
            using (var loader = new CvsLoaderMetadata())
            {
                loader.DbConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AdexMeta;Integrated Security=True;Connect Timeout=3600;";
                loader.OnMessage += Loader_OnMessage;

                return new JsonResult(loader.GetBeneficiary(reference));
            }
        }

        private void Loader_OnMessage(object sender, Common.MessageEventArgs e)
        {
            Debug.Write(e.Message);
        }
    }
}
