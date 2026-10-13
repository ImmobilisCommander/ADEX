using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Mvc.Entities
{
    public sealed class CounterpartyTreemapNodeViewModel
    {
        public string Name { get; set; } = string.Empty;

        public decimal? Value { get; set; }

        public string AmountDisplay { get; set; }

        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CounterpartyTreemapNodeViewModel> Children { get; set; }
    }
}
