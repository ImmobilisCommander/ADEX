using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{

    public sealed class ConcentrationCurve
    {
        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Total { get; set; }

        public double TopOnePercentShare { get; set; }

        public double TopTenShare { get; set; }

        public List<ConcentrationPoint> Points { get; set; } = new();
    }

}
