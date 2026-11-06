using Adex.Mcp.Prompts;
using Adex.Mcp.Tools;
using Adex.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Adex.Mcp.Hosting
{
    public static class McpServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexMcp(this IServiceCollection services)
        {
            services.AddAdexWebApiClient();
            services.AddHealthChecks();
            services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<EntityTools>()
                .WithTools<DashboardTools>()
                .WithPrompts<AdexPrompts>();
            return services;
        }
    }
}
