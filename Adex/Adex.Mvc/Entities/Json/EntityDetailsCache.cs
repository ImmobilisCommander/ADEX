using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.Mvc.Configuration;
using Adex.WebApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.Mvc.Entities.Json
{
    public sealed class EntityDetailsCache(
        IAdexWebApiClient apiClient,
        IMemoryCache cache,
        IOptionsMonitor<MvcCacheOptions> cacheOptions
    )
    {
        public async Task<EntityDetails> GetAsync(
            Guid id,
            int page,
            int pageSize,
            string sort,
            bool descending,
            CancellationToken cancellationToken
        )
        {
            var duration = cacheOptions.CurrentValue.Entity;
            var cacheKey = $"mvc_entity:{id}:{page}:{pageSize}:{sort}:{descending}";

            if (duration > TimeSpan.Zero && cache.TryGetValue(cacheKey, out EntityDetails cached))
            {
                return cached;
            }

            var entity = await apiClient.GetEntityAsync(
                id,
                page,
                pageSize,
                sort,
                descending,
                cancellationToken
            );

            if (entity is not null && duration > TimeSpan.Zero)
            {
                cache.Set(cacheKey, entity, duration);
            }

            return entity;
        }
    }
}
