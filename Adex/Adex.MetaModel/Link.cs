using Adex.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.MetaModel
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        [Key]
        public int Id { get; set; }

        //[Index(IsUnique = true)]
        //[MaxLength(200)]
        //[Required]
        //public string Reference { get; set; }

        [NotMapped]
        public int FromId { get { return From.Id; } }

        public Entity From { get; set; }

        [NotMapped]
        public int ToId { get { return To.Id; } }

        public Entity To { get; set; }
    }
}
