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
        Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default);

        Task<DashboardModel> TryGetDashboardAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EntitySearchResult>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken = default
        );

        Task<EntityDetails> GetEntityAsync(
            Guid id,
            int page = 1,
            int pageSize = 20,
            string sort = "date",
            bool descending = true,
            CancellationToken cancellationToken = default
        );

        Task<EntityCounterpartiesResult> GetEntityCounterpartiesAsync(
            Guid id,
            int? limit = null,
            CancellationToken cancellationToken = default
        );

        Task<JsonElement> GetEntityJsonAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        Task<CsvPage> GetCsvPageAsync(
            int page,
            IReadOnlyDictionary<string, string> filters,
            CancellationToken cancellationToken = default
        );
    }
}
