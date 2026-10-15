namespace Adex.Mvc.Entities.Json
{
    public sealed class EntityTypeBreakdownJsonDto
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }
}
