using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class ConcentrationChartViewModel
    {
        private const double ChartWidth = 480;
        private const double ChartHeight = 300;
        private const double Left = 40;
        private const double Right = 12;
        private const double Top = 12;
        private const double Bottom = 32;
        private static readonly string[] LineClasses = { "chart-line-a", "chart-line-b" };

        public ConcentrationChartViewModel(IReadOnlyList<ConcentrationCurveViewModel> curves)
        {
            var invariantCulture = CultureInfo.InvariantCulture;
            var visibleCurves = curves.Where(curve => curve.Points.Count > 0).ToList();
            Curves = visibleCurves
                .Select(
                    (curve, index) =>
                        (
                            Curve: curve,
                            CssClass: LineClasses[index % LineClasses.Length],
                            Points: string.Join(
                                " ",
                                new[]
                                {
                                    $"{X(0).ToString("0.##", invariantCulture)},{Y(0).ToString("0.##", invariantCulture)}"
                                }.Concat(
                                    curve.Points.Select(point =>
                                        $"{X(point.PopulationPercent).ToString("0.##", invariantCulture)},{Y(point.AmountPercent).ToString("0.##", invariantCulture)}"
                                    )
                                )
                            )
                        )
                )
                .ToList();
            Ticks = new[] { 0, 25, 50, 75, 100 }
                .Select(tick =>
                    (
                        Label: tick.ToString(CultureInfo.InvariantCulture),
                        X: X(tick).ToString("0.##", invariantCulture),
                        Y: Y(tick).ToString("0.##", invariantCulture),
                        AxisY: (Y(tick) + 4).ToString("0.##", invariantCulture)
                    )
                )
                .ToList();
        }

        public IReadOnlyList<(ConcentrationCurveViewModel Curve, string CssClass, string Points)> Curves { get; } =
            Array.Empty<(ConcentrationCurveViewModel, string, string)>();

        public IReadOnlyList<(string Label, string X, string Y, string AxisY)> Ticks { get; } =
            Array.Empty<(string, string, string, string)>();

        public string ViewBox { get; } = $"0 0 {ChartWidth} {ChartHeight}";

        public string DiagonalX1 => X(0).ToString("0.##", CultureInfo.InvariantCulture);

        public string DiagonalY1 => Y(0).ToString("0.##", CultureInfo.InvariantCulture);

        public string DiagonalX2 => X(100).ToString("0.##", CultureInfo.InvariantCulture);

        public string DiagonalY2 => Y(100).ToString("0.##", CultureInfo.InvariantCulture);

        private static double X(double percent) =>
            Left + (ChartWidth - Left - Right) * percent / 100d;

        private static double Y(double percent) =>
            Top + (ChartHeight - Top - Bottom) - (ChartHeight - Top - Bottom) * percent / 100d;
    }
}
