using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Csv;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public sealed class CsvController(ICsvService csvService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<CsvPage>> Get(
            [FromQuery] int page = 1,
            [FromQuery(Name = "filters")] Dictionary<string, string> filters = null,
            CancellationToken cancellationToken = default
        )
        {
            return await csvService.GetPageAsync(page, filters, cancellationToken);
        }
    }
}