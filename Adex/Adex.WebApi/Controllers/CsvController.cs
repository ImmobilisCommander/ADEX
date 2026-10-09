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
    public sealed class CsvController : ControllerBase
    {
        private const int PageSize = 10;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

        private readonly CsvPageReader _reader;
        private readonly IMemoryCache _cache;
        private readonly CsvOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CsvController> _logger;

        public CsvController(
            CsvPageReader reader,
            IMemoryCache cache,
            IOptions<CsvOptions> options,
            IWebHostEnvironment environment,
            ILogger<CsvController> logger
        )
        {
            _reader = reader;
            _cache = cache;
            _options = options.Value;
            _environment = environment;
            _logger = logger;
        }

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

            var path = Path.GetFullPath(_options.FilePath, _environment.ContentRootPath);
            var key = CacheKey(path, Math.Max(page, 1), criteria);
            try
            {
                var result = await _cache.GetOrCreateAsync(
                    key,
                    entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                        return _reader.ReadAsync(path, Math.Max(page, 1), PageSize, criteria, cancellationToken);
                    }
                );
                return result;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                _logger.LogError(exception, "Unable to read the CSV file {Path}", path);
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
