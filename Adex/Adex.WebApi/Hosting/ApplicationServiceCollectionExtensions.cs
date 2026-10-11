using Adex.Business;
using Adex.WebApi.Csv;
using Adex.WebApi.Explorer;
using Adex.WebApi.Import;

using Microsoft.Extensions.DependencyInjection;

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
                options.SwaggerDoc("v1", new() { Title = "ADEX API", Version = "v1" })
            );
            return services;
        }

        public static IServiceCollection AddAdexServices(this IServiceCollection services)
        {
            services.AddScoped<DataExplorerService>();
            services.AddScoped<DeclarationsImporter>();

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
