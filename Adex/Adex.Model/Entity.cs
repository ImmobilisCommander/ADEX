using System.ComponentModel.DataAnnotations;

namespace Adex.Model
{
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
        public string ExternalId { get; set; }

        /// <summary>
        /// Functional designation of the record
        /// </summary>
        [MaxLength(200)]
        public string Designation { get; set; }
    }
}
