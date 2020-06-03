using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    [Table("Persons")]
    public class Person : Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [MaxLength(200)]
        public string LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [MaxLength(200)]
        public string FirstName { get; set; }

        public override string ToString()
        {
            return $"{Reference} {LastName} {FirstName}";
        }
    }
}
