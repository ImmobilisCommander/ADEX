using Adex.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        [NotMapped]
        public int From_Id { get { return From.Id; } }

        [NotMapped]
        public int To_Id { get { return To.Id; } }

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
