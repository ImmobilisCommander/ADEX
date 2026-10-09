using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Adex.Mvc.Models;

namespace Adex.Mvc
{
    public sealed class AdexApiClient
    {
        private readonly HttpClient _httpClient;

        public AdexApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken)
        {
            return GetModelAsync<DashboardViewModel>("api/dashboard", cancellationToken);
        }

        public async Task<DashboardViewModel> TryGetDashboardAsync(
            CancellationToken cancellationToken
        )
        {
            using var response = await _httpClient.GetAsync(
                "api/dashboard/progressive",
                cancellationToken
            );
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DashboardViewModel>(
                    cancellationToken
                )
                ?? throw new InvalidOperationException(
                    "The API returned an empty dashboard response."
                );
        }

        public Task<List<EntitySearchResultViewModel>> SearchEntitiesAsync(
            string query,
            CancellationToken cancellationToken
        )
        {
            return GetModelAsync<List<EntitySearchResultViewModel>>(
                $"api/entity/search?query={Uri.EscapeDataString(query)}",
                cancellationToken
            );
        }

        public async Task<EntityDetailsViewModel> GetEntityAsync(
            Guid id,
            int page,
            int pageSize,
            string sort,
            bool descending,
            CancellationToken cancellationToken
        )
        {
            using var response = await _httpClient.GetAsync(
                $"api/entity/{id}?page={page}&pageSize={pageSize}&sort={Uri.EscapeDataString(sort)}&descending={descending}",
                cancellationToken
            );
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EntityDetailsViewModel>(
                    cancellationToken
                )
                ?? throw new InvalidOperationException(
                    "The API returned an empty entity response."
                );
        }

        public async Task<JsonElement> GetEntityJsonAsync(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            using var response = await _httpClient.GetAsync(
                $"api/entity/{id}",
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken
            );

            return document.RootElement.Clone();
        }

        private async Task<T> GetModelAsync<T>(
            string relativeUri,
            CancellationToken cancellationToken
        )
        {
            using var response = await _httpClient.GetAsync(relativeUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new InvalidOperationException("The API returned an empty response.");
        }
    }
}
