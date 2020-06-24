// <copyright file="ForceDirectedLinkItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Newtonsoft.Json;

namespace Adex.Common
{
    public class ForceDirectedLinkItem
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("nbLinks")]
        public int NbLinks { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
