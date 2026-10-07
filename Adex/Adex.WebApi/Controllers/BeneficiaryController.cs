using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Explorer;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController : ControllerBase
    {
        private readonly DataExplorerService _explorer;

        public BeneficiaryController(DataExplorerService explorer)
        {
            _explorer = explorer;
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
            var entity = await _explorer.GetEntityAsync(reference, cancellationToken);
            return entity is null ? NotFound() : new JsonResult(entity);
        }
    }
}
