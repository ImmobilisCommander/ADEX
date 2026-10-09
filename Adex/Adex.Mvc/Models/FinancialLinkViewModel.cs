using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Adex.Mvc.Models
{

    public sealed class FinancialLinkViewModel
    {
        public Guid Id { get; set; }

        public Guid CounterpartyId { get; set; }

        public string CounterpartyName { get; set; } = string.Empty;

        public string DeclarationType { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public bool Outgoing { get; set; }

        public List<EntityAttributeViewModel> Justification { get; set; } = new();

        public bool HasJustification => Justification.Count > 0;

        public string Motif =>
            Justification.FirstOrDefault(item => item.Name == "Motif")?.Value;

        public string MotifDisplay => string.IsNullOrWhiteSpace(Motif) ? "—" : Motif;

        public string DirectionDisplay => Outgoing ? "Vers" : "Depuis";

        public string KindDisplay => string.IsNullOrWhiteSpace(Kind) ? "—" : Kind;

        public string DateDisplay =>
            Date.ToString("d", CultureInfo.GetCultureInfo("fr-FR"));

        public string AmountDisplay =>
            Amount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"));

        public string DateSortValue => Date.Ticks.ToString(CultureInfo.InvariantCulture);

        public string AmountSortValue => Amount.ToString(CultureInfo.InvariantCulture);

        public string CounterpartyUrl => $"/{CounterpartyId}";

        public string RowClass => HasJustification ? "row-detail" : null;

        public string RowTabIndex => HasJustification ? "0" : null;

        public string RowLabel =>
            HasJustification ? "Voir la justification : " + CounterpartyName : null;

        public string HasJustificationAttribute => HasJustification ? "true" : null;
    }
}
