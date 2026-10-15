using System.Threading.Tasks;
using Adex.Mcp.Prompts;
using Adex.Mcp.Tools;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Adex.Mcp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(
                (context, configuration) =>
                    configuration.ReadFrom.Configuration(context.Configuration)
            );

            builder.Services.AddAdexWebApiClient();
            builder
                .Services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<EntityTools>()
                .WithTools<DashboardTools>()
                .WithPrompts<AdexPrompts>();

            await using var app = builder.Build();
            app.MapMcp("/mcp");
            await app.RunAsync();
        }
    }
}
