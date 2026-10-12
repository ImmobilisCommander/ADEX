using Microsoft.AspNetCore.Mvc;
using Adex.WebApi.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Entities
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController(IAdexWebApiClient apiClient) : ControllerBase
    {
        [HttpGet]
        [Route("{id:guid}")]
        [Route("Read/{id:guid}")]
        public async Task<ActionResult> Read(Guid id, CancellationToken cancellationToken)
        {
            var data = await apiClient.GetEntityJsonAsync(id, cancellationToken);
            return Ok(data);
        }
    }
}
