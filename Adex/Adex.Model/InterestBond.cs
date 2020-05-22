using System;

namespace Adex.Model
{
    public class InterestBond : Entity
    {
        public int ProviderId { get; set; }

        public Entity Provider { get; set; }

        public int BeneficiaryId { get; set; }

        public Entity Beneficiary { get; set; }

        public DateTime DateSignature { get; set; }

        public string Kind { get; set; }

        public decimal Amount { get; set; }

        public override string ToString()
        {
            return Designation;
        }
    }
}
