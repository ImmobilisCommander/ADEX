using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Business
{
    public class DatavizItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("imports")]
        public List<string> Imports { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
