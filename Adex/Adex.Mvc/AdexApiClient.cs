using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Adex.Mvc
{
    public sealed class AdexApiClient
    {
        private readonly HttpClient _httpClient;

        public AdexApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<JsonElement> SearchLinksAsync(string text, CancellationToken cancellationToken)
        {
            return GetJsonAsync($"api/link/search/{Uri.EscapeDataString(text)}", cancellationToken);
        }

        public Task<JsonElement> GetBeneficiaryAsync(
            string reference,
            CancellationToken cancellationToken
        )
        {
            return GetJsonAsync(
                $"api/beneficiary/info/{Uri.EscapeDataString(reference)}",
                cancellationToken
            );
        }

        private async Task<JsonElement> GetJsonAsync(
            string relativeUri,
            CancellationToken cancellationToken
        )
        {
            using var response = await _httpClient.GetAsync(relativeUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken
            );

            return document.RootElement.Clone();
        }
    }
}
