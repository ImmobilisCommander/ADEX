using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class DashboardMonthlyChartViewModel
    {
        private const double ChartWidth = 960;
        private const double ChartHeight = 280;
        private const double Left = 56;
        private const double Right = 8;
        private const double Top = 12;
        private const double Bottom = 28;

        public DashboardMonthlyChartViewModel(IReadOnlyList<MonthlyDeclarationViewModel> months)
        {
            if (months.Count == 0)
            {
                return;
            }

            var plotWidth = ChartWidth - Left - Right;
            var plotHeight = ChartHeight - Top - Bottom;
            var peak = months.Max(month => month.Count);
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(peak, 1))));
            var step = magnitude / 2;
            var axisMax = Math.Ceiling(peak / step) * step;
            var tickCount = Math.Max((int)Math.Round(axisMax / step), 1);
            var slot = plotWidth / months.Count;
            var barWidth = Math.Max(slot * 0.78, 0.6);
            var first = months[0];
            var last = months[^1];
            var peakMonth = months.First(month => month.Count == peak);
            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            var invariantCulture = CultureInfo.InvariantCulture;

            ViewBox = $"0 0 {ChartWidth} {ChartHeight}";
            AccessibleLabel =
                $"Nombre de déclarations par mois, de {MonthName(first, frenchCulture)} à {MonthName(last, frenchCulture)}, maximum {peak.ToString("N0", frenchCulture)} en {MonthName(peakMonth, frenchCulture)}";
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
            Bars = months
                .Select((month, index) =>
                {
                    var x = Left + slot * index + (slot - barWidth) / 2;
                    var height = axisMax == 0 ? 0 : plotHeight * month.Count / axisMax;
                    return (
                        X: x.ToString("0.##", invariantCulture),
                        YearX: (Left + slot * index).ToString("0.##", invariantCulture),
                        Y: (Top + plotHeight - height).ToString("0.##", invariantCulture),
                        Width: barWidth.ToString("0.##", invariantCulture),
                        Height: height.ToString("0.##", invariantCulture),
                        Title: $"{MonthName(month, frenchCulture)} : {month.Count.ToString("N0", frenchCulture)}",
                        Year: month.Month == 1 ? month.Year.ToString(invariantCulture) : null
                    );
                })
                .ToList();
        }

        public string ViewBox { get; } = $"0 0 {ChartWidth} {ChartHeight}";

        public string AccessibleLabel { get; } = string.Empty;

        public IReadOnlyList<(string Y, string LabelY, string Value)> Ticks { get; } =
            Array.Empty<(string, string, string)>();

        public IReadOnlyList<(string X, string YearX, string Y, string Width, string Height, string Title, string Year)> Bars { get; } =
            Array.Empty<(string, string, string, string, string, string, string)>();

        private static string MonthName(MonthlyDeclarationViewModel month, CultureInfo culture)
        {
            return new DateTime(month.Year, month.Month, 1).ToString("MMMM yyyy", culture);
        }
    }
}
