using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    [Table("Companies")]
    public class Company : Entity
    {
        public override string ToString()
        {
            return $"{ExternalId} {Designation}";
        }
    }
}
