using System;
using Adex.Application.Import;
using Adex.Data.Import;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Adex.Importer.Hosting
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexImporter(this IServiceCollection services)
        {
            services
                .AddOptions<ImportOptions>()
                .BindConfiguration(ImportOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton(serviceProvider =>
                new NpgsqlDataSourceBuilder(
                    serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("Adex")
                        ?? throw new InvalidOperationException(
                            "The 'Adex' connection string is not configured."
                        )
                )
                    .EnableDynamicJson()
                    .Build()
            );
            services.AddScoped<IDeclarationsImporter, DeclarationsImporter>();
            services.AddScoped<ImportPreflight>();
            services.AddScoped<ImportRunner>();
            return services;
        }
    }
}
