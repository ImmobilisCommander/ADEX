using System.ComponentModel.DataAnnotations;

namespace Adex.Model
{
    public class Beneficiary : Entity
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
            return $"{ExternalId} {LastName} {FirstName}";
        }
    }
}
