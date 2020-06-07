using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Common
{
    public class DatavizItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("imports")]
        public List<string> Imports { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
