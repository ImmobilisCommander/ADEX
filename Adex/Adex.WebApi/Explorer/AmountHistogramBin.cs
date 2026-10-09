using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{
    // Tranche d'une demi-décade : [LowerBound, UpperBound[. Les montants nuls ou négatifs ont LowerBound = UpperBound = 0.
    public sealed class AmountHistogramBin
    {
        public decimal LowerBound { get; set; }

        public decimal UpperBound { get; set; }

        public int Count { get; set; }
    }
}
