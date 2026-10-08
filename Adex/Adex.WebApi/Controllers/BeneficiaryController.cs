using System;
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("info/{id:guid}")]
        public async Task<ActionResult> GetInformation(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var entity = await _explorer.GetEntityAsync(id, 1, DataExplorerService.DefaultPageSize, "date", true, cancellationToken);
            return entity is null ? NotFound() : new JsonResult(entity);
        }
    }
}
