using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Adex.WebApi.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdexWebApiClient(
            this IServiceCollection services,
            Action<AdexWebApiOptions> configureOptions = null
        )
        {
            var optionsBuilder = services
                .AddOptions<AdexWebApiOptions>()
                .BindConfiguration(AdexWebApiOptions.SectionName)
                .Validate(
                    options =>
                        !string.IsNullOrWhiteSpace(options.BaseAddress)
                        && Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out _),
                    "Le paramètre « AdexApi:BaseAddress » doit être une URL absolue valide."
                )
                .ValidateOnStart();

            if (configureOptions != null)
            {
                optionsBuilder.Configure(configureOptions);
            }

            services
                .AddHttpClient<IAdexWebApiClient, AdexWebApiClient>(
                    (serviceProvider, client) =>
                    {
                        var options = serviceProvider
                            .GetRequiredService<IOptions<AdexWebApiOptions>>()
                            .Value;
                        client.BaseAddress = new Uri(options.BaseAddress);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    }
                )
                .AddResilienceHandler(
                    "adex-api-retry",
                    pipeline =>
                        pipeline.AddRetry(
                            new HttpRetryStrategyOptions
                            {
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
