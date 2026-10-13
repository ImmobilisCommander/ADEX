using Adex.Mvc.Configuration;
using Adex.WebApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adex.Mvc.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexMvc(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMemoryCache();
            services
                .AddOptions<MvcCacheOptions>()
                .BindConfiguration(MvcCacheOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate<IConfiguration>((options, configuration) =>
                {
                    var section = configuration.GetSection(MvcCacheOptions.SectionName);
                    return section.Exists()
                        && !string.IsNullOrWhiteSpace(section[nameof(MvcCacheOptions.Dashboard)])
                        && !string.IsNullOrWhiteSpace(section[nameof(MvcCacheOptions.Entity)])
                        && !string.IsNullOrWhiteSpace(section[nameof(MvcCacheOptions.Counterparties)])
                        && !string.IsNullOrWhiteSpace(section[nameof(MvcCacheOptions.Search)])
                        && !string.IsNullOrWhiteSpace(section[nameof(MvcCacheOptions.Sitemap)]);
                }, "Tous les paramètres de la section « Cache » (Dashboard, Entity, Counterparties, Search, Sitemap) sont obligatoires.")
                .ValidateOnStart();
            services
                .AddOptions<MvcTreemapOptions>()
                .BindConfiguration(MvcTreemapOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate<IConfiguration>(
                    (options, configuration) =>
                        !string.IsNullOrWhiteSpace(
                            configuration.GetSection(MvcTreemapOptions.SectionName)[nameof(MvcTreemapOptions.MaxNodesPerLevel)]
                        ),
                    "Le paramètre « Treemap:MaxNodesPerLevel » est obligatoire."
                )
                .ValidateOnStart();
            services.AddProblemDetails();
            services.AddExceptionHandler<MvcExceptionHandler>();
            return services;
        }

        public static IServiceCollection AddAdexApiClient(this IServiceCollection services)
        {
            return services.AddAdexWebApiClient();
        }
    }
}
