using System.Threading.Tasks;
using Adex.WebApi.Hosting;
using Common.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Adex.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureAdexHost();
            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(serilogLogger);
            builder.Services.AddSingleton<Serilog.ILogger>(serilogLogger);
            builder.Services.AddSingleton(new Serilog.Extensions.Hosting.DiagnosticContext(serilogLogger));
            builder.Services.AddTelemetry(typeof(Program).Assembly.GetName().Name!, "1.0.0").WithAspNetCoreInstrumentation().WithNpgsqlInstrumentation();
            builder.Services.AddAdexWebApi().AddAdexDatabase().AddAdexServices();

            await using var app = builder.Build();
            app.UseAdexPipeline();
            await app.RunAdexAsync();
        }
    }
}
