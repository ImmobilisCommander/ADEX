using Adex.Data.Model;
using Adex.WebApi.Dashboard;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Serilog;

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
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });
            return app;
        }

        public static async Task MigrateDatabaseAsync(
            this WebApplication app,
            CancellationToken cancellationToken
        )
        {
            await using var migrationContext = app
                .Services.GetRequiredService<IDbContextFactory<AdexContext>>()
                .CreateDbContext();
            migrationContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
            var migrations = await migrationContext.Database.GetPendingMigrationsAsync(cancellationToken);

            if (migrations.Any())
            {
                foreach (var migration in migrations)
                {
                    app.Logger.LogInformation("Applying migration: {Migration}", migration);
                    await migrationContext.Database.MigrateAsync(migration, cancellationToken);
                    app.Logger.LogInformation("Migration applied: {Migration}", migration);
                }
            }
            else
            {
                app.Logger.LogInformation("No pending database migrations.");
            }
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
