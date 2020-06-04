using Adex.Common;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        [NotMapped]
        public int FromId { get { return From.Id; } }

        [NotMapped]
        public int ToId { get { return To.Id; } }

        public Entity From { get; set; }

        public Entity To { get; set; }

        public string Kind { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return Reference;
        }
    }
}
