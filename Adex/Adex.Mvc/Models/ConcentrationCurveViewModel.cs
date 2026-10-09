using System;
using System.Collections.Generic;

using System.Globalization;

namespace Adex.Mvc.Models
{

    public sealed class ConcentrationCurveViewModel
    {
        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Total { get; set; }

        public double TopOnePercentShare { get; set; }

        public double TopTenShare { get; set; }

        public List<ConcentrationPointViewModel> Points { get; set; } = new();

        public string TopOnePercentShareDisplay =>
            TopOnePercentShare.ToString("0.#", CultureInfo.GetCultureInfo("fr-FR"));

        public string TopTenShareDisplay =>
            TopTenShare.ToString("0.#", CultureInfo.GetCultureInfo("fr-FR"));

        public string CountDisplay => Count.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));
    }
}
