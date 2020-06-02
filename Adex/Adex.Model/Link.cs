using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    [Table("Links")]
    public class Link : Entity
    {
        public int FromId { get; set; }

        public Entity From { get; set; }

        public int ToId { get; set; }

        public Entity To { get; set; }

        public override string ToString()
        {
            return Designation;
        }
    }
}
