using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    /// <summary>
    /// Serialized dashboard persisted in database; considered stale 24 hours after <see cref="ComputedAtUtc"/>
    /// and deleted by every full import.
    /// </summary>
    [Table("DashboardSnapshots")]
    public class DashboardSnapshot
    {
        [Key]
        public string Key { get; set; }

        public string Json { get; set; }

        public DateTime ComputedAtUtc { get; set; }
    }
}
