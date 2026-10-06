using Adex.Business;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        private readonly IMetadataLookupService _metadataLookupService;

        public MetaController(IMetadataLookupService metadataLookupService)
        {
            _metadataLookupService = metadataLookupService;
        }

        /// <summary>
        /// This method enable user to search for codes of companies or beneficiaries.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/{txt}")]
        public ActionResult Search(string txt)
        {
            return new JsonResult(_metadataLookupService.Search(txt));
        }
    }
}
