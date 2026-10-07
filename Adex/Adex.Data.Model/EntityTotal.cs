using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    /// <summary>
    /// Pre-aggregated financial totals per entity, rebuilt by every full import.
    /// </summary>
    [Table("EntityTotals")]
    public class EntityTotal
    {
        public Guid EntityId { get; set; }

        public int LinkCount { get; set; }

        public decimal OutgoingAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public decimal Total { get; set; }
    }
}
