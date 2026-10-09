using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Csv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Adex.WebApi.Controllers
{
    [ApiController]
    [Route("api/csv")]
    public sealed class CsvController(
        CsvPageReader reader,
        IMemoryCache cache,
        IOptionsMonitor<CsvOptions> options,
        IWebHostEnvironment environment,
        ILogger<CsvController> logger
    ) : ControllerBase
    {
        private const int PageSize = 10;
        private static readonly SemaphoreSlim ScanLock = new(1, 1);

        [HttpGet]
        public async Task<ActionResult<CsvPage>> Get(
            [FromQuery] int page = 1,
            [FromQuery(Name = "filters")] Dictionary<string, string> filters = null,
            CancellationToken cancellationToken = default
        )
        {
            var criteria = (filters ?? new Dictionary<string, string>())
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Key, x => x.Value.Trim(), StringComparer.OrdinalIgnoreCase);
            var forbidden = criteria.Keys.FirstOrDefault(x => !CsvFilterableColumns.Names.Contains(x));
            if (forbidden is not null)
            {
                return Problem(
                    detail: $"La colonne « {forbidden} » ne peut pas être filtrée.",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            var settings = options.CurrentValue;
            var path = Path.GetFullPath(settings.FilePath, environment.ContentRootPath);
            var key = CacheKey(path, Math.Max(page, 1), criteria);
            try
            {
                if (cache.TryGetValue(key, out CsvPage cached))
                {
                    return cached;
                }

                // One scan of the file at a time; waiting requests stop as soon as their client leaves.
                await ScanLock.WaitAsync(cancellationToken);
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
                            return reader.ReadAsync(path, Math.Max(page, 1), PageSize, criteria, cancellationToken);
                        }
                    );
                }
                finally
                {
                    ScanLock.Release();
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                logger.LogError(exception, "Unable to read the CSV file {Path}", path);
                return Problem(
                    detail: $"Le fichier « {Path.GetFileName(path)} » est introuvable ou illisible.",
                    statusCode: StatusCodes.Status404NotFound
                );
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
