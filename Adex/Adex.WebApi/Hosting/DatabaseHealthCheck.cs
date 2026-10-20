using System.Threading;
using System.Threading.Tasks;
using Adex.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Adex.WebApi.Hosting
{
    public sealed class DatabaseHealthCheck(IDbContextFactory<AdexContext> contextFactory)
        : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            await using var db = contextFactory.CreateDbContext();
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Connexion à la base de données impossible.");
        }
    }
}
