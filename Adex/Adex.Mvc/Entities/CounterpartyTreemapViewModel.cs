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
        int maxNodesPerLevel,
        string searchQuery = null
    )
    {
        private static readonly CultureInfo FrenchCulture = new("fr-FR");

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly List<EntityCounterpartySummary> _items = SelectCounterparties(result);

        public string EntityName =>
            !string.IsNullOrWhiteSpace(result?.EntityName) ? result.EntityName
            : string.IsNullOrWhiteSpace(entityName) ? "(sans nom)"
            : entityName;

        public int TotalCounterpartiesCount => result?.TotalCount ?? 0;

        public int DisplayedCount => _items.Count;

        public bool HasCounterparties => _items.Count > 0;

        public string ScopeDisplay =>
            $"{DisplayedCount.ToString("N0", FrenchCulture)} contrepartie{(DisplayedCount > 1 ? "s" : "")}";

        public CounterpartyTreemapDataViewModel Data =>
            new()
            {
                Scope = ScopeDisplay,
                Root = new CounterpartyTreemapNodeViewModel { Name = EntityName, Children = BuildLeaves() },
            };

        public string DataJson => JsonSerializer.Serialize(Data, JsonOptions);

        private static List<EntityCounterpartySummary> SelectCounterparties(EntityCounterpartiesResult result)
        {
            return (result?.Items ?? new List<EntityCounterpartySummary>())
                .Where(item => item.TotalAmount > 0)
                .OrderByDescending(item => item.TotalAmount)
                .ToList();
        }

        private List<CounterpartyTreemapNodeViewModel> BuildLeaves() => BuildLevel(_items);

        // Au-delà de maxNodesPerLevel, les contreparties restantes sont regroupées dans « Autres », récursivement.
        private List<CounterpartyTreemapNodeViewModel> BuildLevel(List<EntityCounterpartySummary> items)
        {
            if (items.Count <= maxNodesPerLevel)
            {
                return items.Select(ToLeaf).ToList();
            }

            var nodes = items.Take(maxNodesPerLevel).Select(ToLeaf).ToList();
            var rest = items.Skip(maxNodesPerLevel).ToList();
            var total = rest.Sum(item => item.TotalAmount);
            nodes.Add(new CounterpartyTreemapNodeViewModel
            {
                Name = "Autres",
                AmountDisplay = total.ToString("C0", FrenchCulture),
                Children = BuildLevel(rest),
            });
            return nodes;
        }

        private CounterpartyTreemapNodeViewModel ToLeaf(EntityCounterpartySummary item) =>
            new()
            {
                Name = item.Name,
                Value = item.TotalAmount,
                AmountDisplay = item.TotalAmount.ToString("C0", FrenchCulture),
                Url = string.IsNullOrWhiteSpace(searchQuery)
                    ? $"/{item.CounterpartyId}"
                    : $"/{item.CounterpartyId}?fromQuery={Uri.EscapeDataString(searchQuery)}",
            };
    }
}
