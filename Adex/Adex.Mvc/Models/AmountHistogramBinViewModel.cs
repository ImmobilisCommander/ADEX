using System;
using System.Collections.Generic;

namespace Adex.Mvc.Models
{

    public sealed class AmountHistogramBinViewModel
    {
        public decimal LowerBound { get; set; }

        public decimal UpperBound { get; set; }

        public int Count { get; set; }
    }
}
