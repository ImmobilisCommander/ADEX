using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Csv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.WebApi.Csv
{
    public sealed class CsvService(
        CsvPageReader reader,
        IMemoryCache cache,
        IOptionsMonitor<CsvOptions> options,
        IWebHostEnvironment environment
    ) : ICsvService
    {
        private const int PageSize = 10;
        private readonly SemaphoreSlim _scanLock = new(1, 1);

        public async Task<CsvPage> GetPageAsync(
            int page,
            Dictionary<string, string> filters,
            CancellationToken cancellationToken
        )
        {
            var criteria = (filters ?? new Dictionary<string, string>())
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Key, x => x.Value.Trim(), StringComparer.OrdinalIgnoreCase);
            var forbidden = criteria.Keys.FirstOrDefault(x => !CsvFilterableColumns.Names.Contains(x));
            if (forbidden is not null)
            {
                throw new CsvFilterNotAllowedException(forbidden);
            }

            var pageNumber = Math.Max(page, 1);
            var path = Path.GetFullPath(options.CurrentValue.FilePath, environment.ContentRootPath);
            var key = CacheKey(path, pageNumber, criteria);
            if (cache.TryGetValue(key, out CsvPage cached))
            {
                return cached;
            }

            // One scan of the file at a time; waiting requests stop as soon as their client leaves.
            await _scanLock.WaitAsync(cancellationToken);
            try
            {
                return await cache.GetOrCreateAsync(
                    key,
                    entry =>
                    {
                        // Read on every scan so that a configuration change applies to the next search.
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(
                            options.CurrentValue.CacheDurationSeconds
                        );
                        return reader.ReadAsync(path, pageNumber, PageSize, criteria, cancellationToken);
                    }
                );
            }
            finally
            {
                _scanLock.Release();
            }
        }

        private static string CacheKey(string path, int page, Dictionary<string, string> criteria)
        {
            var filters = criteria
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .Select(x => $"{x.Key.ToLowerInvariant()}={x.Value.ToLowerInvariant()}");
            return $"csv|{path}|{page}|{string.Join('&', filters)}";
        }
    }
}
