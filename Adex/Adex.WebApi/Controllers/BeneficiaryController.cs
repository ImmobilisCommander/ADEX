using System.Threading;
using System.Threading.Tasks;
using Adex.Business;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController : ControllerBase
    {
        private readonly IMetadataLookupService _metadataLookupService;

        public BeneficiaryController(IMetadataLookupService metadataLookupService)
        {
            _metadataLookupService = metadataLookupService;
        }

        /// <summary>
        /// This method enable user to search for codes of companies or beneficiaries.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("info/{reference}")]
        public async Task<ActionResult> GetInformation(
            string reference,
            CancellationToken cancellationToken
        )
        {
            return new JsonResult(
                await _metadataLookupService.GetBeneficiaryAsync(reference, cancellationToken)
            );
        }
    }
}
