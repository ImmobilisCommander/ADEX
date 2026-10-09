using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
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

        public EntityChartsViewModel EntityCharts => new(this);

        public string FinancialLinkCountDisplay =>
            FinancialLinkCount.ToString("N0", FrenchCulture);

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

        private static CultureInfo FrenchCulture => CultureInfo.GetCultureInfo("fr-FR");

        private (string State, string Url) SortLink(string key)
        {
            var state = Sort == key ? (Descending ? "descending" : "ascending") : null;
            var descending = Sort == key ? !Descending : key is "date" or "amount";
            var url =
                $"/{Id}?page=1&pageSize={PageSize}&sort={key}&descending={descending.ToString().ToLowerInvariant()}#links-title";
            return (state, url);
        }

        private string PageUrl(int number)
        {
            return
                $"/{Id}?page={number}&pageSize={PageSize}&sort={Sort}&descending={DescendingDisplay}#links-title";
        }
    }
}
