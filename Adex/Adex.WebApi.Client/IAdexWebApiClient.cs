using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Csv;
using Adex.Contracts.Explorer;

namespace Adex.WebApi.Client
{
    public interface IAdexWebApiClient
    {
        Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken);

        Task<DashboardModel> TryGetDashboardAsync(CancellationToken cancellationToken);

        Task<IReadOnlyList<EntitySearchResult>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken
        );

        Task<EntityDetails> GetEntityAsync(
            Guid id,
            int page,
            int pageSize,
            string sort,
            bool descending,
            CancellationToken cancellationToken
        );

        Task<EntityCounterpartiesResult> GetEntityCounterpartiesAsync(
            Guid id,
            int? limit,
            CancellationToken cancellationToken
        );

        Task<JsonElement> GetEntityJsonAsync(
            Guid id,
            CancellationToken cancellationToken
        );

        Task<CsvPage> GetCsvPageAsync(
            int page,
            IReadOnlyDictionary<string, string> filters,
            CancellationToken cancellationToken
        );

        Task<SitemapModel> GetSitemapAsync(CancellationToken cancellationToken);
    }
}
