using Adex.Contracts.Explorer;
using Adex.Mvc.Configuration;
using Adex.WebApi.Client;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System;
using System.Threading.Tasks;

namespace Adex.Mvc.Entities
{
    public sealed class CounterpartyTreemapViewComponent(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions,
        IOptionsMonitor<MvcTreemapOptions> treemapOptions
    ) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid entityId, string searchQuery = null)
        {
            var duration = cacheOptions.CurrentValue.Counterparties;
            var cacheKey = $"mvc_counterparties:{entityId}:all";
            EntityCounterpartiesResult result = null;

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityCounterpartiesResult cached))
            {
                result = cached;
            }
            else
            {
                result = await apiClient.GetEntityCounterpartiesAsync(
                    entityId,
                    cancellationToken: HttpContext.RequestAborted
                );

                if (result is not null && duration > TimeSpan.Zero)
                {
                    cache.Set(cacheKey, result, duration);
                }
            }

            var model = new CounterpartyTreemapViewModel(
                null,
                result,
                treemapOptions.CurrentValue.MaxNodesPerLevel,
                searchQuery
            );
            return model.HasCounterparties ? View(model) : Content(string.Empty);
        }
    }
}
