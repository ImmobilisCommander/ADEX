using Adex.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.MetaModel
{
    [Table("Entities")]
    public class Entity : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(200)]
        [Required]
        public string Reference { get; set; }
    }
}
