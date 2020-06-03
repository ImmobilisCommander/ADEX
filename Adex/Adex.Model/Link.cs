using Adex.Interface;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    [Table("Links")]
    public class Link : Entity
    {
        public IEntity From { get; set; }

        public IEntity To { get; set; }

        public override string ToString()
        {
            return Reference;
        }
    }
}
