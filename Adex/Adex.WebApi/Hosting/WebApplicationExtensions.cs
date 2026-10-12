using Adex.Data.Model;
using Adex.WebApi.Dashboard;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using Serilog;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Hosting
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseAdexPipeline(this WebApplication app)
        {
            NpgsqlLoggingConfiguration.InitializeLogging(
                app.Services.GetRequiredService<ILoggerFactory>(),
                parameterLoggingEnabled: false
            );
            app.UseSerilogRequestLogging();
            app.UseExceptionHandler();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.RoutePrefix = string.Empty;
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Adex.WebApi v1");
            });

            app.UseHttpsRedirection();
            app.UseMiddleware<ImportInProgressMiddleware>();
            app.UseAuthorization();
            app.MapControllers();
            return app;
        }

        public static async Task MigrateDatabaseAsync(
            this WebApplication app,
            CancellationToken cancellationToken
        )
        {
            await using var migrationContext = app.Services
                .GetRequiredService<IDbContextFactory<AdexContext>>()
                .CreateDbContext();
            migrationContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            await migrationContext.Database.MigrateAsync(cancellationToken);
        }

        public static async Task RunAdexAsync(this WebApplication app)
        {
            try
            {
                using var shutdown = new CancellationTokenSource();
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };

                await app.MigrateDatabaseAsync(app.Lifetime.ApplicationStopping);
                await app.EnsureDashboardAsync(shutdown.Token);
                await app.RunAsync(shutdown.Token);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
