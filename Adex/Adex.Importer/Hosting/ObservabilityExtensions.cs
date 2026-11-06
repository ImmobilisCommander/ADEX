using Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Adex.Importer.Hosting
{
    public static class ObservabilityExtensions
    {
        public static HostApplicationBuilder AddAdexObservability(this HostApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
            builder.Services.AddSingleton(logger);
            builder.Services.AddSingleton(new Serilog.Extensions.Hosting.DiagnosticContext(logger));
            builder.Services.AddTelemetry(typeof(Program).Assembly.GetName().Name!, "1.0.0")
                .WithNpgsqlInstrumentation();
            return builder;
        }
    }
}
