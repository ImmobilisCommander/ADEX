using System.Globalization;

namespace Adex.Mvc.Entities
{
    public sealed class PlanetaryOrbitViewModel(
        int index,
        double radiusX,
        double radiusY,
        string name,
        bool isPrimary
    )
    {
        public int Index { get; } = index;

        public double RadiusX { get; } = radiusX;

        public double RadiusY { get; } = radiusY;

        public string Name { get; } = name;

        public bool IsPrimary { get; } = isPrimary;

        public string RadiusXDisplay => RadiusX.ToString("0.##", CultureInfo.InvariantCulture);

        public string RadiusYDisplay => RadiusY.ToString("0.##", CultureInfo.InvariantCulture);

        public string CssClass => IsPrimary
            ? "planetary-orbit planetary-orbit-primary"
            : "planetary-orbit planetary-orbit-secondary";
    }
}
