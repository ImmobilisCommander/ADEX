using System;
using System.Collections.Generic;

using System.Globalization;

namespace Adex.Mvc.Models
{

    public sealed class FinancialLinkTypeViewModel
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }

        public string CountDisplay => Count.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));

        public string AmountDisplay => Amount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"));
    }
}
