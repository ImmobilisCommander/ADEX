using Microsoft.Extensions.Hosting;
using Serilog;

namespace Adex.Mvc.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureAdexHost(this IHostBuilder host)
        {
            host.UseDefaultServiceProvider(
                (_, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                }
            );
            host.UseSerilog(
                (context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
            );
            return host;
        }
    }
}
