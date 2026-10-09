using System;
using System.Collections.Generic;

namespace Adex.Mvc.Models
{

    public sealed class MonthlyDeclarationViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Count { get; set; }
    }
}
