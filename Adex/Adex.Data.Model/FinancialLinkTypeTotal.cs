using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    /// <summary>
    /// Pre-aggregated count and amount per declaration type, rebuilt by every full import.
    /// </summary>
    [Table("FinancialLinkTypeTotals")]
    public class FinancialLinkTypeTotal
    {
        [Key]
        public string Type { get; set; }

        public long Count { get; set; }

        public decimal Amount { get; set; }
    }
}
