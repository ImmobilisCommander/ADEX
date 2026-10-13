namespace Adex.Mvc.Entities
{
    public sealed class CounterpartyTreemapDataViewModel
    {
        public string Scope { get; set; } = string.Empty;

        public CounterpartyTreemapNodeViewModel Root { get; set; }
    }
}
