using Adex.Application.Csv;
using Adex.Application.Import;
using Adex.Data.Import;
using Adex.Data.ReadModel.Explorer;
using Adex.WebApi.Dashboard;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Adex.WebApi.Hosting
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexWebApi(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
            );
            services.AddProblemDetails();
            services.AddExceptionHandler<ApiExceptionHandler>();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "ADEX API", Version = "v1" });

                var webApiXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
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
            services.AddScoped<IDeclarationsImporter, DeclarationsImporter>();

            services.AddOptions<DashboardOptions>()
                .BindConfiguration(DashboardOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddOptions<CsvOptions>()
                .BindConfiguration(CsvOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<CsvPageReader>();
            services.AddSingleton<ICsvService, CsvService>();

            services.AddOptions<ImportOptions>()
                .BindConfiguration(ImportOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddSingleton<ImportJobService>();
            return services;
        }
    }
}
