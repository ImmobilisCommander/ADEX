using Microsoft.Extensions.Hosting;

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
            return host;
        }
    }
}
