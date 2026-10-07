using Adex.Business;
using Adex.Common;
using Adex.Data.MetaModel;
using Adex.Data.Model;
using Adex.WebApi.Explorer;
using Adex.WebApi.Import;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using Serilog;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseDefaultServiceProvider(
                (_, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                }
            );
            builder.Host.UseSerilog(
                (context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
            );

            builder.Services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter()
                )
            );
            builder.Services.AddDbContextFactory<AdexContext>((serviceProvider, options) =>
                options.UseNpgsql(
                    GetRequiredConnectionString(
                        serviceProvider.GetRequiredService<IConfiguration>(),
                        "Adex"
                    )
                )
            );
            builder.Services.AddDbContextFactory<AdexMetaContext>((serviceProvider, options) =>
                options.UseNpgsql(
                    GetRequiredConnectionString(
                        serviceProvider.GetRequiredService<IConfiguration>(),
                        "AdexMeta"
                    )
                )
            );
            builder.Services.AddScoped<DataExplorerService>();
            builder.Services.AddScoped(provider =>
            {
                var loader = new CsvLoaderNormalized(
                    provider.GetRequiredService<IDbContextFactory<AdexContext>>()
                );
                AttachLogging(provider.GetRequiredService<ILogger<CsvLoaderNormalized>>(), loader);
                return loader;
            });
            builder.Services.AddScoped<ILinkSearchService>(provider =>
                provider.GetRequiredService<CsvLoaderNormalized>()
            );
            builder.Services.AddScoped<MetadataConsolidationService>();

            builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection("Import"));
            builder.Services.AddSingleton<ImportJobService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
                options.SwaggerDoc("v1", new() { Title = "ADEX API", Version = "v1" })
            );

            var app = builder.Build();
            NpgsqlLoggingConfiguration.InitializeLogging(
                app.Services.GetRequiredService<ILoggerFactory>(),
                parameterLoggingEnabled: false
            );
            app.UseSerilogRequestLogging();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            try
            {
                using var shutdown = new CancellationTokenSource();
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };

                await app.Services.GetRequiredService<IDbContextFactory<AdexContext>>().CreateDbContext().Database.MigrateAsync(app.Lifetime.ApplicationStopping);

                await app.RunAsync(shutdown.Token);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static string GetRequiredConnectionString(
            IConfiguration configuration,
            string name
        )
        {
            return configuration.GetConnectionString(name)
                ?? throw new InvalidOperationException(
                    $"The '{name}' connection string is not configured."
                );
        }

        private static void AttachLogging<T>(ILogger<T> logger, ICsvLoader loader)
        {
            loader.OnMessage += (_, message) =>
            {
                switch (message.Level)
                {
                    case Level.Debug:
                        logger.LogDebug("{LoaderMessage}", message.Message);
                        break;
                    case Level.Info:
                        logger.LogInformation("{LoaderMessage}", message.Message);
                        break;
                    case Level.Warn:
                        logger.LogWarning("{LoaderMessage}", message.Message);
                        break;
                    case Level.Error:
                        logger.LogError("{LoaderMessage}", message.Message);
                        break;
                }
            };
        }
    }
}
