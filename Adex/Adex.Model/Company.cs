using System.ComponentModel.DataAnnotations;

namespace Adex.Model
{
    public class Company : Entity
    {
        public override string ToString()
        {
            return $"{ExternalId} {Designation}";
        }
    }
}
