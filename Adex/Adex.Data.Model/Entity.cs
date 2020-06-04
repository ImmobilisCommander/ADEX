using Adex.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
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
        [MaxLength(200)]
        public string Reference { get; set; }
    }
}
