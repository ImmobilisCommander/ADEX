using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.MetaModel
{
    [Table("Members")]
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(200)]
        [Required]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Alias { get; set; }
    }
}
