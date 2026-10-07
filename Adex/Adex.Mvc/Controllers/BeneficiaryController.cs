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
        [Route("{id}")]
        [Route("Read/{id}")]
        public async Task<ActionResult> Read(string id, CancellationToken cancellationToken)
        {
            var data = await _apiClient.GetBeneficiaryAsync(id, cancellationToken);
            return new JsonResult(data);
        }
    }
}
