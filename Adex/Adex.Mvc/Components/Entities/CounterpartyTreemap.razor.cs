using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.Mvc.Configuration;
using Adex.Mvc.Entities;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;

namespace Adex.Mvc.Components.Entities
{
    public partial class CounterpartyTreemap
    {
        private CounterpartyTreemapViewModel model;
        private string dataJson;
        private string initializedJson;
        private ElementReference section;
        private IJSObjectReference module;

        [Parameter, EditorRequired]
        public Guid EntityId { get; set; }

        [Parameter]
        public string SearchQuery { get; set; }

        protected override Task OnParametersSetAsync() => RunWithLoadingAsync(LoadAsync);

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            var duration = CacheOptions.CurrentValue.Counterparties;
            var cacheKey = $"mvc_counterparties:{EntityId}:all";

            if (!(duration > TimeSpan.Zero && Cache.TryGetValue(cacheKey, out EntityCounterpartiesResult result)))
            {
                result = await ApiClient.GetEntityCounterpartiesAsync(EntityId, cancellationToken: cancellationToken);
                if (result is not null && duration > TimeSpan.Zero)
                {
                    Cache.Set(cacheKey, result, duration);
                }
            }

            model = new CounterpartyTreemapViewModel(
                null,
                result,
                TreemapOptions.CurrentValue.MaxNodesPerLevel,
                SearchQuery
            );
            dataJson = model.HasCounterparties ? model.DataJson : null;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (dataJson is null || dataJson == initializedJson)
            {
                return;
            }

            initializedJson = dataJson;
            module ??= await JS.InvokeAsync<IJSObjectReference>("import", "/js/entity-treemap.js");
            await module.InvokeVoidAsync("init", section);
        }

        public async ValueTask DisposeAsync()
        {
            Dispose();
            if (module is not null)
            {
                try
                {
                    await module.InvokeVoidAsync("dispose");
                    await module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                    // Le circuit est déjà fermé : le navigateur a libéré le module.
                }
            }
        }
    }
}
