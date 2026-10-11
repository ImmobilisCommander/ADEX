using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adex.Data.ReadModel.Explorer;

namespace Adex.WebApi.Dashboard
{
    public static class DashboardStartupExtensions
    {
        /// <summary>
        /// Generates the dashboard snapshot if it is missing or expired. Call it before <c>RunAsync</c>:
        /// the server does not accept requests until the snapshot is ready.
        /// </summary>
        public static async Task EnsureDashboardAsync(
            this WebApplication app,
            CancellationToken cancellationToken
        )
        {
            await using var scope = app.Services.CreateAsyncScope();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DashboardStartupExtensions));

            logger.LogInformation("Checking the dashboard snapshot");
            await scope.ServiceProvider
                .GetRequiredService<DataExplorerService>()
                .EnsureDashboardAsync(cancellationToken);
            logger.LogInformation("Dashboard snapshot is up to date");
        }
    }
}
