using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Entities
{
    public sealed class PlanetaryCounterpartyViewModel(
        Guid id,
        string name,
        decimal amount,
        bool hasIncoming,
        bool hasOutgoing,
        string searchQuery,
        double maximumAmount,
        int index,
        int countInOrbit,
        double orbitRadiusX,
        double orbitRadiusY,
        double angleOffset,
        bool showPermanentLabel,
        int orbitIndex
    )
    {
        private static readonly CultureInfo FrenchCulture = CultureInfo.GetCultureInfo("fr-FR");

        public Guid Id { get; } = id;

        public string Name { get; } = name;

        public decimal Amount { get; } = amount;

        public string AmountDisplay => Amount.ToString("C0", FrenchCulture);

        public IReadOnlyList<string> TextLines { get; } = WrapName(name);

        public string Tone => hasIncoming && hasOutgoing
            ? "planetary-satellite-mixed"
            : hasOutgoing ? "planetary-satellite-outgoing" : "planetary-satellite-incoming";

        public bool ShowPermanentLabel { get; } = showPermanentLabel;

        public int OrbitIndex { get; } = orbitIndex;

        public string LabelCssClass => ShowPermanentLabel
            ? "planetary-label planetary-label-permanent"
            : "planetary-label planetary-label-hover";

        public double Radius { get; } = showPermanentLabel
            ? 12d + 14d * Math.Sqrt(maximumAmount <= 0d ? 0d : (double)amount / maximumAmount)
            : 6d + 8d * Math.Sqrt(maximumAmount <= 0d ? 0d : (double)amount / maximumAmount);

        public double X { get; } = GetX(index, countInOrbit, orbitRadiusX, angleOffset);

        public double Y { get; } = GetY(index, countInOrbit, orbitRadiusY, angleOffset);

        public double LabelY { get; } = GetLabelY(index, countInOrbit, orbitRadiusY, angleOffset);

        public string XDisplay => X.ToString("0.##", CultureInfo.InvariantCulture);

        public string YDisplay => Y.ToString("0.##", CultureInfo.InvariantCulture);

        public string LabelYDisplay => LabelY.ToString("0.##", CultureInfo.InvariantCulture);

        public string RadiusDisplay => Radius.ToString("0.##", CultureInfo.InvariantCulture);

        public string FirstLineOffset =>
            ((1 - TextLines.Count) * 8d).ToString("0.##", CultureInfo.InvariantCulture);

        public string SearchQuery { get; set; } = searchQuery;

        public string CounterpartyUrl =>
            string.IsNullOrWhiteSpace(SearchQuery)
                ? $"/{Id}"
                : $"/{Id}?fromQuery={Uri.EscapeDataString(SearchQuery)}";

        public string AccessibleLabel => $"{Name} : {AmountDisplay}";

        private static double GetAngle(int index, int count, double offset) =>
            -Math.PI / 2d + offset + 2d * Math.PI * index / Math.Max(count, 1);

        private static double GetX(int index, int count, double radiusX, double offset) =>
            480d + radiusX * Math.Cos(GetAngle(index, count, offset));

        private static double GetY(int index, int count, double radiusY, double offset) =>
            320d + radiusY * Math.Sin(GetAngle(index, count, offset));

        private static double GetLabelY(int index, int count, double radiusY, double offset)
        {
            var angle = GetAngle(index, count, offset);
            var outsideOffset = Math.Sin(angle) < -0.35d ? -28d : 32d;
            return GetY(index, count, radiusY, offset) + outsideOffset;
        }

        private static IReadOnlyList<string> WrapName(string name)
        {
            const int maximumLineLength = 18;
            var lines = new List<string>();
            var line = string.Empty;

            foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length > 0 && line.Length + word.Length + 1 > maximumLineLength)
                {
                    lines.Add(line);
                    line = word;
                }
                else
                {
                    line = line.Length == 0 ? word : line + " " + word;
                }
            }

            if (line.Length > 0)
            {
                lines.Add(line);
            }

            return lines;
        }
    }
}
