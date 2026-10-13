using Adex.Contracts.Explorer;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Entities
{
    public sealed class EntityDetailsViewModel
    {
        public string ErrorMessage { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal OutgoingAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public int FinancialLinkCount { get; set; }

        public int Page { get; set; } = 1;

        public string Sort { get; set; } = "date";

        public bool Descending { get; set; } = true;

        public int PageSize { get; set; }

        public int PageCount { get; set; } = 1;

        public List<EntityYearActivityViewModel> YearlyActivity { get; set; } = new();

        public List<EntityTypeBreakdownViewModel> TypeBreakdown { get; set; } = new();

        public List<EntityAttributeViewModel> Attributes { get; set; } = new();

        public List<FinancialLinkViewModel> FinancialLinks { get; set; } = new();

        public PlanetarySystemViewModel PlanetarySystem { get; set; }

        public List<PlanetaryCounterpartyViewModel> PlanetaryCounterparties => PlanetarySystem?.Counterparties.ToList() ?? new();

        public List<PlanetaryOrbitViewModel> PlanetaryOrbits => PlanetarySystem?.Orbits.ToList() ?? new();

        public IReadOnlyList<EntityCounterpartySummary> TopCounterparties { get; private set; } = new List<EntityCounterpartySummary>();

        public const int DefaultPlanetaryLimit = 12;

        public int PlanetaryLimit { get; set; } = DefaultPlanetaryLimit;

        public int TotalCounterpartiesCount { get; set; }

        public string PlanetarySystemViewBox => PlanetarySystem?.ViewBox ?? "0 0 960 640";

        public bool PlanetaryIsGlobalScope { get; private set; }

        public string PlanetaryLinkScopeDisplay => PlanetarySystem?.LinkScopeDisplay ?? string.Empty;

        public IReadOnlyList<string> PlanetaryCoreNameLines => PlanetarySystem?.CoreNameLines ?? new List<string>();

        public string PlanetaryCoreFirstLineOffset => PlanetarySystem?.CoreFirstLineOffset ?? "0";

        public string SearchQuery { get; set; }

        public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchQuery);

        public bool ShowSearchLink => HasSearchQuery;

        public string SearchUrl =>
            HasSearchQuery
                ? $"/Search?query={Uri.EscapeDataString(SearchQuery)}"
                : "/Search";

        public EntityChartsViewModel EntityCharts => new(this);

        public string FinancialLinkCountDisplay => FinancialLinkCount.ToString("N0", FrenchCulture);

        public string IncomingAmountDisplay => IncomingAmount.ToString("C0", FrenchCulture);

        public string OutgoingAmountDisplay => OutgoingAmount.ToString("C0", FrenchCulture);

        public string AttributeCountDisplay => Attributes.Count.ToString("N0", FrenchCulture);

        public string DescendingDisplay => Descending.ToString().ToLowerInvariant();

        public string PageAction => $"/{Id}";

        public int FirstDisplayedLink => (Page - 1) * PageSize + 1;

        public int LastDisplayedLink => FirstDisplayedLink + FinancialLinks.Count - 1;

        public IReadOnlyDictionary<string, (string State, string Url)> SortLinks =>
            new Dictionary<string, (string State, string Url)>
            {
                ["direction"] = SortLink("direction"),
                ["name"] = SortLink("name"),
                ["type"] = SortLink("type"),
                ["kind"] = SortLink("kind"),
                ["date"] = SortLink("date"),
                ["amount"] = SortLink("amount"),
            };

        public IReadOnlyList<(int Number, string Url, bool Current, bool GapBefore)> PageLinks
        {
            get
            {
                var firstCandidate = Math.Max(1, Page - 2);
                var lastCandidate = Math.Min(PageCount, Page + 2);
                var pages = new[] { 1, PageCount }
                    .Concat(
                        Enumerable.Range(
                            firstCandidate,
                            Math.Max(0, lastCandidate - firstCandidate + 1)
                        )
                    )
                    .Distinct()
                    .OrderBy(number => number)
                    .ToList();

                return pages
                    .Select(
                        (number, index) =>
                            (
                                Number: number,
                                Url: PageUrl(number),
                                Current: number == Page,
                                GapBefore: index > 0 && number - pages[index - 1] > 1
                            )
                    )
                    .ToList();
            }
        }

        public string PreviousPageUrl => PageUrl(Page - 1);

        public string NextPageUrl => PageUrl(Page + 1);

        public IReadOnlyList<int> PageSizeOptions { get; } = new[] { 10, 20, 50 };

        public void BuildPlanetarySystem(
            EntityCounterpartiesResult counterpartiesResult = null,
            int? limit = null
        )
        {
            var requestedLimit = limit is > 0
                ? limit.Value
                : DefaultPlanetaryLimit;

            if (counterpartiesResult is { Items: { Count: > 0 } })
            {
                PlanetarySystem = PlanetarySystemViewModel.Create(
                    Id,
                    Name,
                    counterpartiesResult,
                    requestedLimit,
                    SearchQuery
                );
                PlanetaryIsGlobalScope = true;
                TotalCounterpartiesCount = PlanetarySystem.TotalCounterpartiesCount;
                PlanetaryLimit = PlanetarySystem.Limit;
                TopCounterparties = counterpartiesResult.Items.Take(PlanetaryLimit).ToList();
                return;
            }

            PlanetaryIsGlobalScope = false;
            var groups = FinancialLinks
                .GroupBy(link => link.CounterpartyId)
                .Select(group => new EntityCounterpartySummary
                {
                    CounterpartyId = group.Key,
                    Name = group.First().CounterpartyName,
                    TotalAmount = group.Sum(link => link.Amount),
                    IncomingAmount = group.Where(link => !link.Outgoing).Sum(link => link.Amount),
                    OutgoingAmount = group.Where(link => link.Outgoing).Sum(link => link.Amount),
                    LinkCount = group.Count(),
                })
                .OrderByDescending(group => group.TotalAmount)
                .ThenBy(group => group.Name, StringComparer.CurrentCulture)
                .ToList();

            TotalCounterpartiesCount = groups.Count;
            PlanetaryLimit = limit is > 0
                ? Math.Clamp(limit.Value, 1, Math.Max(1, TotalCounterpartiesCount))
                : Math.Min(DefaultPlanetaryLimit, Math.Max(1, TotalCounterpartiesCount));
            TopCounterparties = groups.Take(PlanetaryLimit).ToList();

            PlanetarySystem = new PlanetarySystemViewModel(
                Id,
                Name,
                TopCounterparties,
                TotalCounterpartiesCount,
                PlanetaryLimit,
                isGlobalScope: false,
                searchQuery: SearchQuery
            );
        }

        private static CultureInfo FrenchCulture => CultureInfo.GetCultureInfo("fr-FR");

        private (string State, string Url) SortLink(string key)
        {
            var state = Sort == key ? (Descending ? "descending" : "ascending") : null;
            var descending = Sort == key ? !Descending : key is "date" or "amount";
            var queryParam = HasSearchQuery ? $"&fromQuery={Uri.EscapeDataString(SearchQuery)}" : string.Empty;
            var limitParam = PlanetaryLimit != DefaultPlanetaryLimit ? $"&limit={PlanetaryLimit}" : string.Empty;
            var url =
                $"/{Id}?page=1&pageSize={PageSize}&sort={key}&descending={descending.ToString().ToLowerInvariant()}{queryParam}{limitParam}#links-title";
            return (state, url);
        }

        private string PageUrl(int number)
        {
            var queryParam = HasSearchQuery ? $"&fromQuery={Uri.EscapeDataString(SearchQuery)}" : string.Empty;
            var limitParam = PlanetaryLimit != DefaultPlanetaryLimit ? $"&limit={PlanetaryLimit}" : string.Empty;
            return $"/{Id}?page={number}&pageSize={PageSize}&sort={Sort}&descending={DescendingDisplay}{queryParam}{limitParam}#links-title";
        }
    }
}
