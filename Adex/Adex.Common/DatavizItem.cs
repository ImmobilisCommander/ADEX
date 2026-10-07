// <copyright file="DatavizItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Common
{
    public class EdgeBundlingItem
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
