using System;
using System.Collections.Generic;
using System.Globalization;

namespace Adex.Mvc.Models
{

    public sealed class EntitySearchResultViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public int LinkCount { get; set; }

        public decimal Amount { get; set; }

        public Dictionary<string, string> Attributes { get; set; } = new();

        public string LinkCountDisplay => LinkCount.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));

        public string AmountDisplay => Amount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"));

        public string AttributeValue(string name) =>
            Attributes.TryGetValue(name, out var value) ? value : string.Empty;
    }
}
