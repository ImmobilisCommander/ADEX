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
    public sealed class DashboardController : ControllerBase
    {
        private readonly DataExplorerService _explorer;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            DataExplorerService explorer,
            ILogger<DashboardController> logger
        )
        {
            _explorer = explorer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardModel>> Get(CancellationToken cancellationToken)
        {
            return await _explorer.GetDashboardAsync(cancellationToken);
        }

        [HttpGet("progressive")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<DashboardModel>> GetProgressive()
        {
            try
            {
                var dashboard = await _explorer.TryGetDashboardAsync();
                return dashboard is null ? Accepted() : dashboard;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to calculate dashboard data");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Le calcul des données du tableau de bord a échoué." }
                );
            }
        }
    }
}
