using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController(AdexApiClient apiClient) : ControllerBase
    {
        [HttpGet]
        [Route("{id:guid}")]
        [Route("Read/{id:guid}")]
        public async Task<ActionResult> Read(Guid id, CancellationToken cancellationToken)
        {
            var data = await apiClient.GetEntityJsonAsync(id, cancellationToken);
            return new JsonResult(data);
        }
    }
}
