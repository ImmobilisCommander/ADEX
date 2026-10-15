using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Adex.Importer.Hosting
{
    public static class HostBuilderExtensions
    {
        public static HostApplicationBuilder ConfigureAdexImporterHost(
            this HostApplicationBuilder builder
        )
        {
            builder.ConfigureContainer(
                new DefaultServiceProviderFactory(
                    new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
                )
            );
            builder.Services.AddSerilog(
                (services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(services)
            );
            return builder;
        }
    }
}
