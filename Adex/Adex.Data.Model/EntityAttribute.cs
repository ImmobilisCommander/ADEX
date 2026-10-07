using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("EntityAttributes")]
    public class EntityAttribute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EntityId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Value { get; set; }

        public Entity Entity { get; set; }
    }
}
