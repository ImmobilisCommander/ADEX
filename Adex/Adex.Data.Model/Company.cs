using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("Companies")]
    public class Company : Entity
    {
        /// <summary>
        /// Functional designation of the record
        /// </summary>
        [MaxLength(200)]
        public virtual string Designation { get; set; }

        public override string ToString()
        {
            return $"{Reference} {Designation}";
        }
    }
}
