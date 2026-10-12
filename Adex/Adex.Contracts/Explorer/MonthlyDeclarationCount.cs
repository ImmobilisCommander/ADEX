using System;
using System.Collections.Generic;

namespace Adex.Contracts.Explorer
{
    public sealed class MonthlyDeclarationCount
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Count { get; set; }
    }
}
