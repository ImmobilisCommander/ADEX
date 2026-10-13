using Adex.Contracts.Explorer;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Entities
{
    public sealed class PlanetarySystemViewModel(
        Guid entityId,
        string entityName,
        IReadOnlyList<EntityCounterpartySummary> items,
        int totalCount,
        int requestedLimit,
        bool isGlobalScope = true,
        string searchQuery = null
    )
    {
        private static readonly CultureInfo FrenchCulture = new("fr-FR");

        public Guid EntityId => entityId;

        public string EntityName => string.IsNullOrWhiteSpace(entityName) ? "(sans nom)" : entityName;

        public string SearchQuery => searchQuery;

        public bool IsGlobalScope => isGlobalScope;

        public CounterpartyLimitSliderViewModel Slider { get; set; }

        public int TotalCounterpartiesCount => totalCount;

        public int Limit => totalCount > 0
            ? Math.Clamp(requestedLimit, 1, Math.Max(1, totalCount))
            : Math.Max(1, requestedLimit);

        private readonly (IReadOnlyList<PlanetaryOrbitViewModel> Orbits, IReadOnlyList<PlanetaryCounterpartyViewModel> Counterparties) _layout =
            BuildLayout(
                string.IsNullOrWhiteSpace(entityName) ? "(sans nom)" : entityName,
                items,
                totalCount,
                requestedLimit,
                searchQuery
            );

        public IReadOnlyList<PlanetaryOrbitViewModel> Orbits => _layout.Orbits;

        public IReadOnlyList<PlanetaryCounterpartyViewModel> Counterparties => _layout.Counterparties;

        public bool HasCounterparties => Counterparties.Count > 0;

        public string ViewBox => "0 0 960 640";

        public string LinkScopeDisplay =>
            TotalCounterpartiesCount > 0
                ? $"{Counterparties.Count.ToString("N0", FrenchCulture)} contrepartie{(Counterparties.Count > 1 ? "s" : "")} affichée{(Counterparties.Count > 1 ? "s" : "")} sur {TotalCounterpartiesCount.ToString("N0", FrenchCulture)}"
                : $"{Counterparties.Count.ToString("N0", FrenchCulture)} contrepartie{(Counterparties.Count > 1 ? "s" : "")} affichée{(Counterparties.Count > 1 ? "s" : "")}";

        public IReadOnlyList<string> CoreNameLines => WrapName(EntityName, 16).Take(3).ToList();

        public string CoreFirstLineOffset =>
            ((1 - CoreNameLines.Count) * 9d).ToString(
                "0.##",
                CultureInfo.InvariantCulture
            );

        public static PlanetarySystemViewModel Create(
            Guid entityId,
            string entityName,
            EntityCounterpartiesResult result,
            int requestedLimit,
            string searchQuery = null
        )
        {
            if (result is { Items.Count: > 0 })
            {
                var resolvedName = !string.IsNullOrWhiteSpace(result.EntityName) ? result.EntityName : entityName;
                return new PlanetarySystemViewModel(
                    entityId,
                    resolvedName,
                    result.Items,
                    result.TotalCount,
                    requestedLimit,
                    isGlobalScope: true,
                    searchQuery: searchQuery
                );
            }

            return new PlanetarySystemViewModel(
                entityId,
                entityName,
                items: Array.Empty<EntityCounterpartySummary>(),
                totalCount: 0,
                requestedLimit: requestedLimit,
                isGlobalScope: false,
                searchQuery: searchQuery
            );
        }

        private static (IReadOnlyList<PlanetaryOrbitViewModel> Orbits, IReadOnlyList<PlanetaryCounterpartyViewModel> Counterparties) BuildLayout(
            string entityName,
            IReadOnlyList<EntityCounterpartySummary> rawItems,
            int totalCount,
            int requestedLimit,
            string searchQuery
        )
        {
            var orbits = new List<PlanetaryOrbitViewModel>();
            var counterparties = new List<PlanetaryCounterpartyViewModel>();

            if (rawItems == null || rawItems.Count == 0)
            {
                return (orbits, counterparties);
            }

            var effectiveLimit = totalCount > 0
                ? Math.Clamp(requestedLimit, 1, Math.Max(1, totalCount))
                : Math.Max(1, requestedLimit);

            var items = rawItems.Take(effectiveLimit).ToList();
            if (items.Count == 0)
            {
                return (orbits, counterparties);
            }

            var maximumAmount = (double)items.Max(item => item.TotalAmount);
            var total = items.Count;

            if (total <= 8)
            {
                var orbit = new PlanetaryOrbitViewModel(0, 350d, 220d, "Orbite principale", isPrimary: true);
                orbits.Add(orbit);

                for (var i = 0; i < total; i++)
                {
                    var item = items[i];
                    counterparties.Add(new PlanetaryCounterpartyViewModel(
                        item.CounterpartyId,
                        item.Name,
                        item.TotalAmount,
                        item.IncomingAmount > 0,
                        item.OutgoingAmount > 0,
                        searchQuery,
                        maximumAmount,
                        i,
                        total,
                        orbit.RadiusX,
                        orbit.RadiusY,
                        angleOffset: 0d,
                        showPermanentLabel: true,
                        orbitIndex: 0
                    ));
                }
                return (orbits, counterparties);
            }

            if (total <= 18)
            {
                var countOuter = Math.Min(8, total);
                var countInner = total - countOuter;

                var outerOrbit = new PlanetaryOrbitViewModel(0, 395d, 255d, "Orbite extérieure", isPrimary: true);
                var innerOrbit = new PlanetaryOrbitViewModel(1, 240d, 150d, "Orbite intérieure", isPrimary: false);
                orbits.Add(innerOrbit);
                orbits.Add(outerOrbit);

                for (var i = 0; i < countInner; i++)
                {
                    var item = items[countOuter + i];
                    counterparties.Add(new PlanetaryCounterpartyViewModel(
                        item.CounterpartyId,
                        item.Name,
                        item.TotalAmount,
                        item.IncomingAmount > 0,
                        item.OutgoingAmount > 0,
                        searchQuery,
                        maximumAmount,
                        i,
                        countInner,
                        innerOrbit.RadiusX,
                        innerOrbit.RadiusY,
                        angleOffset: Math.PI / Math.Max(countInner, 1),
                        showPermanentLabel: false,
                        orbitIndex: 1
                    ));
                }

                for (var i = 0; i < countOuter; i++)
                {
                    var item = items[i];
                    counterparties.Add(new PlanetaryCounterpartyViewModel(
                        item.CounterpartyId,
                        item.Name,
                        item.TotalAmount,
                        item.IncomingAmount > 0,
                        item.OutgoingAmount > 0,
                        searchQuery,
                        maximumAmount,
                        i,
                        countOuter,
                        outerOrbit.RadiusX,
                        outerOrbit.RadiusY,
                        angleOffset: 0d,
                        showPermanentLabel: true,
                        orbitIndex: 0
                    ));
                }
                return (orbits, counterparties);
            }

            var countTier1 = Math.Min(10, Math.Max(8, (int)Math.Round(total * 0.22)));
            var remainingAfterTier1 = total - countTier1;
            var countTier2 = Math.Max(6, (int)Math.Round(remainingAfterTier1 * 0.45));
            var countTier3 = remainingAfterTier1 - countTier2;

            var outer = new PlanetaryOrbitViewModel(0, 395d, 255d, "Orbite extérieure", isPrimary: true);
            var middle = new PlanetaryOrbitViewModel(1, 285d, 180d, "Orbite médiane", isPrimary: false);
            var inner = new PlanetaryOrbitViewModel(2, 180d, 115d, "Orbite intérieure", isPrimary: false);
            orbits.Add(inner);
            orbits.Add(middle);
            orbits.Add(outer);

            for (var i = 0; i < countTier3; i++)
            {
                var item = items[countTier1 + countTier2 + i];
                counterparties.Add(new PlanetaryCounterpartyViewModel(
                    item.CounterpartyId,
                    item.Name,
                    item.TotalAmount,
                    item.IncomingAmount > 0,
                    item.OutgoingAmount > 0,
                    searchQuery,
                    maximumAmount,
                    i,
                    countTier3,
                    inner.RadiusX,
                    inner.RadiusY,
                    angleOffset: Math.PI / (2 * Math.Max(countTier3, 1)),
                    showPermanentLabel: false,
                    orbitIndex: 2
                ));
            }

            for (var i = 0; i < countTier2; i++)
            {
                var item = items[countTier1 + i];
                counterparties.Add(new PlanetaryCounterpartyViewModel(
                    item.CounterpartyId,
                    item.Name,
                    item.TotalAmount,
                    item.IncomingAmount > 0,
                    item.OutgoingAmount > 0,
                    searchQuery,
                    maximumAmount,
                    i,
                    countTier2,
                    middle.RadiusX,
                    middle.RadiusY,
                    angleOffset: Math.PI / Math.Max(countTier2, 1),
                    showPermanentLabel: false,
                    orbitIndex: 1
                ));
            }

            for (var i = 0; i < countTier1; i++)
            {
                var item = items[i];
                counterparties.Add(new PlanetaryCounterpartyViewModel(
                    item.CounterpartyId,
                    item.Name,
                    item.TotalAmount,
                    item.IncomingAmount > 0,
                    item.OutgoingAmount > 0,
                    searchQuery,
                    maximumAmount,
                    i,
                    countTier1,
                    outer.RadiusX,
                    outer.RadiusY,
                    angleOffset: 0d,
                    showPermanentLabel: true,
                    orbitIndex: 0
                ));
            }

            return (orbits, counterparties);
        }

        private static IReadOnlyList<string> WrapName(string name, int maximumLineLength)
        {
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
