using System;
using System.Threading.Tasks;
using Adex.Importer.Hosting;
using Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Adex.Importer
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(
                new HostApplicationBuilderSettings
                {
                    Args = args,
                    ContentRootPath = AppContext.BaseDirectory,
                }
            );
            builder.ConfigureAdexImporterHost();
            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(serilogLogger);
            builder.Services.AddSingleton(serilogLogger);
            builder.Services.AddSingleton(new Serilog.Extensions.Hosting.DiagnosticContext(serilogLogger));
            builder.Services.AddTelemetry(typeof(Program).Assembly.GetName().Name!, "1.0.0").WithNpgsqlInstrumentation();
            builder.Services.AddAdexImporter();

            using var host = builder.Build();
            return await host.RunAdexImporterAsync(args);
        }
    }
}
