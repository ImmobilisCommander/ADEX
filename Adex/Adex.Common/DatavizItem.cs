// <copyright file="DatavizItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adex.Common
{
    public class EdgeBundlingItem
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
