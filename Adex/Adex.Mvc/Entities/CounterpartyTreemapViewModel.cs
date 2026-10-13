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

        // Profondeur maximale de la chaîne d'« Autres » ; la sérialisation JSON l'autorise explicitement.
        private const int MaxLevels = 1000;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { MaxDepth = MaxLevels * 2 + 16 };

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

        // Chaque niveau affiche les contreparties les plus élevées puis un unique « Autres » pour le reste.
        // Construction itérative (du niveau le plus profond vers la racine) : pas de récursion sur des milliers de niveaux.
        private List<CounterpartyTreemapNodeViewModel> BuildLevel(List<EntityCounterpartySummary> items)
        {
            if (items.Count <= maxNodesPerLevel)
            {
                return items.Select(ToLeaf).ToList();
            }

            // Le nombre de contreparties par niveau garantit une profondeur au plus égale à MaxLevels.
            var visible = Math.Max(Math.Max(1, maxNodesPerLevel - 1), (int)Math.Ceiling(items.Count / (double)MaxLevels));

            var offsets = new List<int>();
            for (var offset = 0; items.Count - offset > maxNodesPerLevel; offset += visible)
            {
                offsets.Add(offset);
            }

            var lastOffset = offsets[^1] + visible;
            var nodes = items.Skip(lastOffset).Select(ToLeaf).ToList();
            for (var i = offsets.Count - 1; i >= 0; i--)
            {
                var level = items.Skip(offsets[i]).Take(visible).Select(ToLeaf).ToList();
                var rest = items.Skip(offsets[i] + visible).ToList();
                level.Add(ToOthers(rest, nodes));
                nodes = level;
            }

            return nodes;
        }

        private static CounterpartyTreemapNodeViewModel ToOthers(
            List<EntityCounterpartySummary> items, List<CounterpartyTreemapNodeViewModel> children) =>
            new()
            {
                Name = "Autres",
                AmountDisplay = items.Sum(item => item.TotalAmount).ToString("C0", FrenchCulture),
                Children = children,
            };

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
