using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Csv;
using Adex.Contracts.Explorer;

namespace Adex.WebApi.Client
{
    public sealed class AdexWebApiClient(HttpClient httpClient) : IAdexWebApiClient
    {
        public Task<DashboardModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            return GetModelAsync<DashboardModel>("api/dashboard", cancellationToken);
        }

        public async Task<DashboardModel> TryGetDashboardAsync(
            CancellationToken cancellationToken = default
        )
        {
            using var response = await httpClient.GetAsync(
                "api/dashboard/progressive",
                cancellationToken
            );
            if (response.StatusCode is HttpStatusCode.Accepted or HttpStatusCode.ServiceUnavailable)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DashboardModel>(cancellationToken)
                ?? throw new InvalidOperationException(
                    "The API returned an empty dashboard response."
                );
        }

        public async Task<IReadOnlyList<EntitySearchResult>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken = default
        )
        {
            var results = await GetModelAsync<List<EntitySearchResult>>(
                $"api/entity/search?query={Uri.EscapeDataString(query)}",
                cancellationToken
            );
            return results;
        }

        public async Task<EntityDetails> GetEntityAsync(
            Guid id,
            int page = 1,
            int pageSize = 20,
            string sort = "date",
            bool descending = true,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await httpClient.GetAsync(
                $"api/entity/{id}?page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}&descending={descending}",
                cancellationToken
            );
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EntityDetails>(cancellationToken)
                ?? throw new InvalidOperationException(
                    "The API returned an empty entity response."
                );
        }

        public async Task<EntityCounterpartiesResult> GetEntityCounterpartiesAsync(
            Guid id,
            int? limit = null,
            CancellationToken cancellationToken = default
        )
        {
            var uri = limit is > 0
                ? $"api/entity/{id}/counterparties?limit={limit.Value}"
                : $"api/entity/{id}/counterparties";

            using var response = await httpClient.GetAsync(uri, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EntityCounterpartiesResult>(
                cancellationToken
            ) ?? throw new InvalidOperationException(
                "The API returned an empty counterparties response."
            );
        }

        public async Task<JsonElement> GetEntityJsonAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await httpClient.GetAsync($"api/entity/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken
            );
            return document.RootElement.Clone();
        }

        public async Task<CsvPage> GetCsvPageAsync(
            int page,
            IReadOnlyDictionary<string, string> filters,
            CancellationToken cancellationToken = default
        )
        {
            var query = new List<string> { $"page={page}" };
            if (filters != null)
            {
                query.AddRange(
                    filters.Select(x =>
                        $"filters[{Uri.EscapeDataString(x.Key)}]={Uri.EscapeDataString(x.Value)}"
                    )
                );
            }

            using var response = await httpClient.GetAsync(
                $"api/csv?{string.Join('&', query)}",
                cancellationToken
            );
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                var message = "Le fichier CSV est inaccessible.";
                try
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(
                        cancellationToken
                    );
                    using var doc = await JsonDocument.ParseAsync(
                        stream,
                        cancellationToken: cancellationToken
                    );
                    if (
                        doc.RootElement.TryGetProperty("detail", out var detail)
                        && detail.GetString() is { } detailStr
                    )
                    {
                        message = detailStr;
                    }
                    else if (
                        doc.RootElement.TryGetProperty("title", out var title)
                        && title.GetString() is { } titleStr
                    )
                    {
                        message = titleStr;
                    }
                }
                catch (JsonException) { }

                throw new AdexApiException(message, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CsvPage>(cancellationToken)
                ?? throw new InvalidOperationException("The API returned an empty CSV response.");
        }

        public Task<SitemapModel> GetSitemapAsync(CancellationToken cancellationToken = default)
        {
            return GetModelAsync<SitemapModel>("api/sitemap", cancellationToken);
        }

        private async Task<T> GetModelAsync<T>(
            string relativeUri,
            CancellationToken cancellationToken
        )
        {
            using var response = await httpClient.GetAsync(relativeUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new InvalidOperationException("The API returned an empty response.");
        }
    }
}
