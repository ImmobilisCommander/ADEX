using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController : ControllerBase
    {
        private readonly AdexApiClient _apiClient;

        public BeneficiaryController(AdexApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        [Route("{id:guid}")]
        [Route("Read/{id:guid}")]
        public async Task<ActionResult> Read(Guid id, CancellationToken cancellationToken)
        {
            var data = await _apiClient.GetEntityJsonAsync(id, cancellationToken);
            return new JsonResult(data);
        }
    }
}
