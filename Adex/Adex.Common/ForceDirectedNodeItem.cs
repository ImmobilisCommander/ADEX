// <copyright file="ForceDirectedNodeItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Newtonsoft.Json;

namespace Adex.Common
{
    public class ForceDirectedNodeItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }
    }
}
