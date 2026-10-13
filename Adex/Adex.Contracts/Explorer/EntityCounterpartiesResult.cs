using System;
using System.Collections.Generic;

namespace Adex.Contracts.Explorer
{
    public sealed class EntityCounterpartiesResult
    {
        public Guid EntityId { get; set; }

        public string EntityName { get; set; } = string.Empty;

        public int TotalCount { get; set; }

        public List<EntityCounterpartySummary> Items { get; set; } = new();
    }
}
