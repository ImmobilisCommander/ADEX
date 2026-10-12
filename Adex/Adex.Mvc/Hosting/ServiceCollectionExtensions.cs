using Adex.WebApi.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Adex.Mvc.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexMvc(this IServiceCollection services)
        {
            services.AddControllersWithViews();
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
