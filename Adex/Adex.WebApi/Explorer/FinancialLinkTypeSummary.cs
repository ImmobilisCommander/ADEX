using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{

    public sealed class FinancialLinkTypeSummary
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }
}
