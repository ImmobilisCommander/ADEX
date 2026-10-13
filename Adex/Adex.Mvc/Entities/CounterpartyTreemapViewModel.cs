using Adex.Contracts.Explorer;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Adex.Mvc.Entities
{
    public sealed class CounterpartyTreemapViewModel(
        string entityName,
        EntityCounterpartiesResult result,
        string searchQuery = null
    )
    {
        public const double DisplayedShare = 0.2;

        private static readonly CultureInfo FrenchCulture = new("fr-FR");

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly List<EntityCounterpartySummary> _items = SelectTopShare(result);

        public string EntityName =>
            !string.IsNullOrWhiteSpace(result?.EntityName) ? result.EntityName
            : string.IsNullOrWhiteSpace(entityName) ? "(sans nom)"
            : entityName;

        public int TotalCounterpartiesCount => result?.TotalCount ?? 0;

        public int DisplayedCount => _items.Count;

        public bool HasCounterparties => _items.Count > 0;

        public string ScopeDisplay =>
            $"{DisplayedCount.ToString("N0", FrenchCulture)} contrepartie{(DisplayedCount > 1 ? "s" : "")} les plus élevées sur {TotalCounterpartiesCount.ToString("N0", FrenchCulture)}";

        public CounterpartyTreemapDataViewModel Data =>
            new()
            {
                Scope = ScopeDisplay,
                Root = new CounterpartyTreemapNodeViewModel { Name = EntityName, Children = BuildLeaves() },
            };

        public string DataJson => JsonSerializer.Serialize(Data, JsonOptions);

        private static List<EntityCounterpartySummary> SelectTopShare(EntityCounterpartiesResult result)
        {
            var items = (result?.Items ?? new List<EntityCounterpartySummary>())
                .Where(item => item.TotalAmount > 0)
                .OrderByDescending(item => item.TotalAmount)
                .ToList();

            var count = (int)Math.Ceiling(Math.Max(result?.TotalCount ?? 0, items.Count) * DisplayedShare);
            return items.Take(Math.Max(count, 1)).ToList();
        }

        private List<CounterpartyTreemapNodeViewModel> BuildLeaves() =>
            _items
                .Select(item => new CounterpartyTreemapNodeViewModel
                {
                    Name = item.Name,
                    Value = item.TotalAmount,
                    AmountDisplay = item.TotalAmount.ToString("C0", FrenchCulture),
                    Url = string.IsNullOrWhiteSpace(searchQuery)
                        ? $"/{item.CounterpartyId}"
                        : $"/{item.CounterpartyId}?fromQuery={Uri.EscapeDataString(searchQuery)}",
                })
                .ToList();
    }
}
