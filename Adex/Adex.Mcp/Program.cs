using System.Threading.Tasks;
using Adex.Mcp.Prompts;
using Adex.Mcp.Tools;
using Adex.WebApi.Client;
using Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Adex.Mcp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(serilogLogger);
            builder.Services.AddSingleton(serilogLogger);
            builder.Services.AddSingleton(new Serilog.Extensions.Hosting.DiagnosticContext(serilogLogger));
            builder.Services.AddTelemetry(typeof(Program).Assembly.GetName().Name!, "1.0.0");

            builder.Services.AddAdexWebApiClient();
            builder.Services.AddHealthChecks();
            builder
                .Services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<EntityTools>()
                .WithTools<DashboardTools>()
                .WithPrompts<AdexPrompts>();

            await using var app = builder.Build();
            app.MapMcp("/mcp");
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready");
            await app.RunAsync();
        }
    }
}
