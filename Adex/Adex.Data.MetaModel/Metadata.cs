using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.MetaModel
{
    [Table("Metadatas")]
    public class Metadata
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Entity Entity { get; set; }

        [Required]
        public Member Member { get; set; }

        public string Value { get; set; }
    }
}
