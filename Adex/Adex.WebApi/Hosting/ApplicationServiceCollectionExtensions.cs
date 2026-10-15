using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Adex.Application.Configuration;
using Adex.Application.Csv;
using Adex.Application.Search;
using Adex.Data.ReadModel.Explorer;
using Adex.WebApi.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adex.WebApi.Hosting
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexWebApi(this IServiceCollection services)
        {
            services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
                );
            services.AddProblemDetails();
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "ADEX API", Version = "v1" });

                var webApiXml = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
                );
                if (File.Exists(webApiXml))
                {
                    options.IncludeXmlComments(webApiXml, includeControllerXmlComments: true);
                }

                var contractsXml = Path.Combine(AppContext.BaseDirectory, "Adex.Contracts.xml");
                if (File.Exists(contractsXml))
                {
                    options.IncludeXmlComments(contractsXml);
                }
            });
            return services;
        }

        public static IServiceCollection AddAdexServices(this IServiceCollection services)
        {
            services.AddScoped<DataExplorerService>();

            services
                .AddOptions<DashboardOptions>()
                .BindConfiguration(DashboardOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services
                .AddOptions<CacheOptions>()
                .BindConfiguration(CacheOptions.SectionName)
                .ValidateDataAnnotations()
                .Validate<IConfiguration>((options, configuration) =>
                {
                    var section = configuration.GetSection(CacheOptions.SectionName);
                    return section.Exists()
                        && !string.IsNullOrWhiteSpace(section[nameof(CacheOptions.Dashboard)])
                        && !string.IsNullOrWhiteSpace(section[nameof(CacheOptions.Entity)])
                        && !string.IsNullOrWhiteSpace(section[nameof(CacheOptions.Counterparties)])
                        && !string.IsNullOrWhiteSpace(section[nameof(CacheOptions.Search)])
                        && !string.IsNullOrWhiteSpace(section[nameof(CacheOptions.Sitemap)]);
                }, "Tous les paramètres de la section « Cache » (Dashboard, Entity, Counterparties, Search, Sitemap) sont obligatoires.")
                .ValidateOnStart();
            services
                .AddOptions<CsvOptions>()
                .BindConfiguration(CsvOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<CsvPageReader>();
            services.AddSingleton<ICsvService, CsvService>();

            services
                .AddOptions<SearchOptions>()
                .BindConfiguration(SearchOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            return services;
        }
    }
}
