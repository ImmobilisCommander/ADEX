using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Model
{
    [Table("FinancialLinks")]
    public class FinancialLink : Link
    {
        public decimal Amount { get; set; }
    }
}
