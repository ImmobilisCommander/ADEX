using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    /// <summary>
    /// All the descriptive attributes of an entity (company, beneficiary or link), stored as a
    /// single jsonb document keyed by attribute name.
    /// </summary>
    [Table("EntityAttributes")]
    public class EntityAttribute
    {
        [Key]
        public Guid EntityId { get; set; }

        [Required]
        [Column("Data", TypeName = "jsonb")]
        public Dictionary<string, string> Data { get; set; } = new();

        public Entity Entity { get; set; }
    }
}
