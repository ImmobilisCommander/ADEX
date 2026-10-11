using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Explorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adex.WebApi.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public sealed class DashboardController(DataExplorerService explorer) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<DashboardModel>> Get(CancellationToken cancellationToken)
        {
            return await explorer.GetDashboardAsync(cancellationToken);
        }

        [HttpGet("progressive")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<DashboardModel>> GetProgressive(CancellationToken cancellationToken)
        {
            var dashboard = await explorer.TryGetDashboardAsync(cancellationToken);
            return dashboard is null ? Accepted() : dashboard;
        }
    }
}
