using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Contracts.Explorer
{

    public sealed class FinancialLinkDetails
    {
        public Guid Id { get; set; }

        public Guid CounterpartyId { get; set; }

        public string CounterpartyName { get; set; } = string.Empty;

        public string DeclarationType { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public bool Outgoing { get; set; }

        [JsonIgnore]
        public string SortName { get; set; }

        public List<EntityAttributeDetails> Justification { get; set; } = new();
    }
}
