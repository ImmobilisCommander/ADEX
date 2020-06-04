using Adex.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.MetaModel
{
    [Table("Entities")]
    public class Entity : IEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// External identifier
        /// </summary>
        [Index(IsUnique = true)]
        [MaxLength(200)]
        [Required]
        public string Reference { get; set; }
    }
}
