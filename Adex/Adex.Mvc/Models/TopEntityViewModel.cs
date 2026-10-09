using System.Globalization;
using System;
using System.Collections.Generic;

namespace Adex.Mvc.Models
{

    public sealed class TopEntityViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string AmountDisplay => Amount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"));
    }
}
