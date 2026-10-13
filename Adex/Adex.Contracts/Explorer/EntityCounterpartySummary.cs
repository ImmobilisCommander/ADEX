using System;

namespace Adex.Contracts.Explorer
{
    public sealed class EntityCounterpartySummary
    {
        public Guid CounterpartyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public decimal OutgoingAmount { get; set; }

        public int LinkCount { get; set; }
    }
}
