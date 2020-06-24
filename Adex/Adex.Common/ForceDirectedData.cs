// <copyright file="ForceDirectedData.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Adex.Common
{
    public class ForceDirectedData
    {
        [JsonProperty("links")]
        public List<ForceDirectedLinkItem> ForceDirectedLinks { get; private set; }

        [JsonProperty("nodes")]
        public List<ForceDirectedNodeItem> ForceDirectedNodes { get; private set; }

        public ForceDirectedData()
        {
            ForceDirectedLinks = new List<ForceDirectedLinkItem>();
            ForceDirectedNodes = new List<ForceDirectedNodeItem>();
        }
    }
}
