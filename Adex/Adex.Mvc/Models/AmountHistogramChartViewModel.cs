using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class AmountHistogramChartViewModel
    {
        private const double ChartWidth = 480;
        private const double ChartHeight = 300;
        private const double Left = 52;
        private const double Right = 8;
        private const double Top = 12;
        private const double Bottom = 40;

        public AmountHistogramChartViewModel(IReadOnlyList<AmountHistogramBinViewModel> bins)
        {
            if (bins.Count == 0)
            {
                return;
            }

            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            var invariantCulture = CultureInfo.InvariantCulture;
            var plotWidth = ChartWidth - Left - Right;
            var plotHeight = ChartHeight - Top - Bottom;
            var peak = bins.Max(bin => bin.Count);
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(peak, 1))));
            var step = magnitude / 2;
            var axisMax = Math.Ceiling(peak / step) * step;
            var tickCount = Math.Max((int)Math.Round(axisMax / step), 1);
            var slot = plotWidth / bins.Count;
            var barWidth = Math.Max(slot * 0.8, 0.6);
            var modal = bins.First(bin => bin.Count == peak);
            AccessibleLabel =
                $"Distribution des montants par tranche logarithmique ; tranche la plus fréquente : {Range(modal, frenchCulture)}, {peak.ToString("N0", frenchCulture)} liens";

            Ticks = Enumerable
                .Range(0, tickCount + 1)
                .Select(tick =>
                {
                    var value = axisMax * tick / tickCount;
                    var y = Top + plotHeight - plotHeight * tick / (double)tickCount;
                    return (
                        Y: y.ToString("0.##", invariantCulture),
                        LabelY: (y + 4).ToString("0.##", invariantCulture),
                        Value: value.ToString("N0", frenchCulture)
                    );
                })
                .ToList();
            Bars = bins
                .Select((bin, index) =>
                {
                    var x = Left + slot * index + (slot - barWidth) / 2;
                    var height = axisMax == 0 ? 0 : plotHeight * bin.Count / axisMax;
                    return (
                        X: x.ToString("0.##", invariantCulture),
                        Y: (Top + plotHeight - height).ToString("0.##", invariantCulture),
                        Width: barWidth.ToString("0.##", invariantCulture),
                        Height: height.ToString("0.##", invariantCulture),
                        Title: $"{Range(bin, frenchCulture)} : {bin.Count.ToString("N0", frenchCulture)} liens",
                        ShowLabel: IsDecade(bin) || bin.UpperBound == 0m,
                        Label: bin.UpperBound == 0m ? "≤ 0" : Money(bin.LowerBound, frenchCulture)
                    );
                })
                .ToList();
            HasNonPositiveBin = bins.Any(bin => bin.UpperBound == 0m);
        }

        public string ViewBox { get; } = $"0 0 {ChartWidth} {ChartHeight}";

        public string AccessibleLabel { get; } = string.Empty;

        public IReadOnlyList<(string Y, string LabelY, string Value)> Ticks { get; } =
            Array.Empty<(string, string, string)>();

        public IReadOnlyList<(string X, string Y, string Width, string Height, string Title, bool ShowLabel, string Label)> Bars { get; } =
            Array.Empty<(string, string, string, string, string, bool, string)>();

        public bool HasNonPositiveBin { get; }

        private static string Money(decimal value, CultureInfo culture)
        {
            return value >= 1_000_000m
                ? (value / 1_000_000m).ToString("0.##", culture) + " M€"
                : value >= 1_000m
                    ? (value / 1_000m).ToString("0.##", culture) + " k€"
                    : value.ToString("0.##", culture) + " €";
        }

        private static string Range(AmountHistogramBinViewModel bin, CultureInfo culture)
        {
            return bin.UpperBound == 0m
                ? "montant nul ou négatif"
                : $"{Money(bin.LowerBound, culture)} à {Money(bin.UpperBound, culture)}";
        }

        private static bool IsDecade(AmountHistogramBinViewModel bin)
        {
            if (bin.LowerBound <= 0m)
            {
                return false;
            }

            var log = Math.Log10((double)bin.LowerBound);
            return Math.Abs(log - Math.Round(log)) < 0.01;
        }
    }
}
