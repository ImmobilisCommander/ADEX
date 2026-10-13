using Adex.Contracts.Explorer;
using Adex.Mvc.Configuration;
using Adex.WebApi.Client;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adex.Mvc.Entities
{
    public sealed class PlanetarySystemViewComponent(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions
    ) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            Guid entityId,
            int limit,
            string searchQuery = null,
            string formAction = null,
            IReadOnlyDictionary<string, string> formState = null
        )
        {
            var requestedLimit = limit > 0 ? limit : EntityDetailsViewModel.DefaultCounterpartyLimit;
            var duration = cacheOptions.CurrentValue.Counterparties;
            var cacheKey = $"mvc_counterparties:{entityId}:{requestedLimit}";
            EntityCounterpartiesResult result = null;

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityCounterpartiesResult cached))
            {
                result = cached;
            }
            else
            {
                result = await apiClient.GetEntityCounterpartiesAsync(
                    entityId,
                    limit: requestedLimit,
                    cancellationToken: HttpContext.RequestAborted
                );

                if (result is not null && duration > TimeSpan.Zero)
                {
                    cache.Set(cacheKey, result, duration);
                }
            }

            var model = PlanetarySystemViewModel.Create(entityId, null, result, requestedLimit, searchQuery);
            if (!model.HasCounterparties)
            {
                return Content(string.Empty);
            }

            model.Slider = new CounterpartyLimitSliderViewModel(
                "planetary-limit-slider",
                $"/{entityId}/planetary",
                formAction ?? $"/{entityId}",
                model.Limit,
                model.TotalCounterpartiesCount,
                formState
            );

            return View(model);
        }
    }
}
