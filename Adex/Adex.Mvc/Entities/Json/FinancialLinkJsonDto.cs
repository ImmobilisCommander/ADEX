using System;
using System.Collections.Generic;

namespace Adex.Mvc.Entities.Json
{
    public sealed class FinancialLinkJsonDto
    {
        public Guid Id { get; set; }

        public Guid CounterpartyId { get; set; }

        public string CounterpartyName { get; set; } = string.Empty;

        public string DeclarationType { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public bool Outgoing { get; set; }

        public List<EntityAttributeJsonDto> Justification { get; set; } = new();
    }
}
