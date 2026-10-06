// <copyright file="ForceDirectedLinkItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Text.Json.Serialization;

namespace Adex.Common
{
    public class ForceDirectedLinkItem
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("nbLinks")]
        public int NbLinks { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
