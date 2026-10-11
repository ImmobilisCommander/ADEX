using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

using Polly;

using System;
using System.Threading;

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
            services.AddOptions<AdexApiOptions>()
                .BindConfiguration(AdexApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddHttpClient<AdexApiClient>((serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(
                    serviceProvider.GetRequiredService<IOptions<AdexApiOptions>>().Value.BaseAddress
                );
                // Scanning the whole CSV can be long; the call is cancelled when the user leaves the page.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddResilienceHandler(
                "adex-api-retry",
                pipeline =>
                    pipeline.AddRetry(
                        new HttpRetryStrategyOptions
                        {
                            // Retry until the API answers; the request cancellation token stops it.
                            MaxRetryAttempts = int.MaxValue,
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            Delay = TimeSpan.FromSeconds(1),
                            MaxDelay = TimeSpan.FromSeconds(30),
                        }
                    )
            );
            return services;
        }
    }
}
