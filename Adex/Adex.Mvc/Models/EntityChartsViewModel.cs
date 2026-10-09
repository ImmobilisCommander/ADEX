using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class EntityChartsViewModel
    {
        private const double ChartWidth = 480;
        private const double ChartHeight = 220;
        private const double Left = 64;
        private const double Right = 8;
        private const double Top = 12;
        private const double Bottom = 28;

        public EntityChartsViewModel(EntityDetailsViewModel entity)
        {
            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            var invariantCulture = CultureInfo.InvariantCulture;
            var years = entity.YearlyActivity;
            var breakdown = entity.TypeBreakdown;
            HasYearlyActivity = years.Count > 0;
            HasTypeBreakdown = breakdown.Count > 0;

            if (HasYearlyActivity)
            {
                var plotWidth = ChartWidth - Left - Right;
                var plotHeight = ChartHeight - Top - Bottom;
                var peak = (double)years.Max(item => Math.Max(item.Amount, 0m));
                var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(peak, 1))));
                var step = magnitude / 2;
                var axisMax = Math.Max(Math.Ceiling(peak / step) * step, step);
                var tickCount = Math.Max((int)Math.Round(axisMax / step), 1);
                var slot = plotWidth / years.Count;
                var barWidth = Math.Min(Math.Max(slot * 0.7, 2), 40);
                var labelEvery = Math.Max(1, (int)Math.Ceiling(years.Count / 8d));
                YearlyChartLabel = $"Montants déclarés par année, de {years[0].Year} à {years[^1].Year}";
                YearTicks = Enumerable
                    .Range(0, tickCount + 1)
                    .Select(tick =>
                    {
                        var value = axisMax * tick / tickCount;
                        var y = Top + plotHeight - plotHeight * tick / (double)tickCount;
                        return (
                            Y: y.ToString("0.##", invariantCulture),
                            LabelY: (y + 4).ToString("0.##", invariantCulture),
                            Value: Compact(value, frenchCulture)
                        );
                    })
                    .ToList();
                YearBars = years
                    .Select((item, index) =>
                    {
                        var x = Left + slot * index + (slot - barWidth) / 2;
                        var height = plotHeight * (double)Math.Max(item.Amount, 0m) / axisMax;
                        return (
                            X: x.ToString("0.##", invariantCulture),
                            LabelX: (Left + slot * index + slot / 2).ToString("0.##", invariantCulture),
                            Y: (Top + plotHeight - height).ToString("0.##", invariantCulture),
                            Width: barWidth.ToString("0.##", invariantCulture),
                            Height: height.ToString("0.##", invariantCulture),
                            Title: $"{item.Year} : {item.Amount.ToString("C0", frenchCulture)} · {item.Count.ToString("N0", frenchCulture)} liens",
                            Year: index % labelEvery == 0 ? item.Year.ToString(invariantCulture) : null
                        );
                    })
                    .ToList();
            }

            var totalCount = breakdown.Sum(item => item.Count);
            var totalAmount = breakdown.Sum(item => Math.Max(item.Amount, 0m));
            TypeBreakdown = breakdown
                .Select(
                    (item, index) =>
                        (
                            Item: item,
                            CssClass: $"segment-{index % 4}",
                            CountShare: Share(item.Count, totalCount, invariantCulture),
                            AmountShare: Share(
                                (double)Math.Max(item.Amount, 0m),
                                (double)totalAmount,
                                invariantCulture
                            ),
                            CountTitle: $"{item.Type} : {item.Count.ToString("N0", frenchCulture)} liens",
                            AmountTitle: $"{item.Type} : {item.Amount.ToString("C0", frenchCulture)}"
                        )
                )
                .ToList();
        }

        public bool HasYearlyActivity { get; }

        public bool HasTypeBreakdown { get; }

        public bool HasContent => HasYearlyActivity || HasTypeBreakdown;

        public string YearlyChartLabel { get; } = string.Empty;

        public IReadOnlyList<(string Y, string LabelY, string Value)> YearTicks { get; } =
            Array.Empty<(string, string, string)>();

        public IReadOnlyList<(string X, string LabelX, string Y, string Width, string Height, string Title, string Year)> YearBars { get; } =
            Array.Empty<(string, string, string, string, string, string, string)>();

        public IReadOnlyList<(EntityTypeBreakdownViewModel Item, string CssClass, string CountShare, string AmountShare, string CountTitle, string AmountTitle)> TypeBreakdown { get; } =
            Array.Empty<(EntityTypeBreakdownViewModel, string, string, string, string, string)>();

        public string YearlyChartViewBox { get; } = $"0 0 {ChartWidth} {ChartHeight}";

        private static string Share(double value, double total, CultureInfo culture) =>
            total <= 0 ? "0" : (value * 100d / total).ToString("0.##", culture);

        private static string Compact(double value, CultureInfo culture) =>
            value >= 1_000_000d
                ? (value / 1_000_000d).ToString("0.##", culture) + " M€"
                : value >= 1_000d
                    ? (value / 1_000d).ToString("0.##", culture) + " k€"
                    : value.ToString("0", culture) + " €";
    }
}
