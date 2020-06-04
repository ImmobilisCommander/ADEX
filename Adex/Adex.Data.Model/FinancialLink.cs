using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("FinancialLinks")]
    public class FinancialLink : Link
    {
        public decimal Amount { get; set; }
    }
}
