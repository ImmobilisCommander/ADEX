using Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Adex.Mvc.Hosting
{
    public static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddAdexObservability(this WebApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
            builder.Services.AddSingleton<Serilog.ILogger>(logger);
            builder.Services.AddSingleton(new Serilog.Extensions.Hosting.DiagnosticContext(logger));
            builder.Services.AddTelemetry(typeof(Program).Assembly.GetName().Name!, "1.0.0")
                .WithAspNetCoreInstrumentation();
            return builder;
        }
    }
}
