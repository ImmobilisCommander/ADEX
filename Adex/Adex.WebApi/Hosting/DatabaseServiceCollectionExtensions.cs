using System;
using Adex.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Adex.WebApi.Hosting
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexDatabase(this IServiceCollection services)
        {
            services.AddSingleton(serviceProvider => new NpgsqlDataSourceBuilder(
                    new NpgsqlConnectionStringBuilder(
                        GetRequiredConnectionString(
                            serviceProvider.GetRequiredService<IConfiguration>(),
                            "Adex"
                        )
                    )
                    {
                        // L'import rouvre des connexions depuis la chaîne exposée par le DbContext.
                        PersistSecurityInfo = true,
                    }.ConnectionString
                ).EnableDynamicJson().Build());
            services.AddDbContextFactory<AdexContext>(
                (serviceProvider, options) =>
                    options
                        .UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );
            return services;
        }

        private static string GetRequiredConnectionString(IConfiguration configuration, string name)
        {
            return configuration.GetConnectionString(name)
                ?? throw new InvalidOperationException(
                    $"The '{name}' connection string is not configured."
                );
        }
    }
}
