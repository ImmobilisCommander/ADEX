using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Explorer;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public sealed class DashboardController : ControllerBase
    {
        private readonly DataExplorerService _explorer;

        public DashboardController(DataExplorerService explorer)
        {
            _explorer = explorer;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardModel>> Get(CancellationToken cancellationToken)
        {
            return await _explorer.GetDashboardAsync(cancellationToken);
        }
    }
}
