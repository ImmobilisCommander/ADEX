// <copyright file="ForceDirectedData.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adex.Common
{
    public class ForceDirectedData
    {
        [JsonPropertyName("links")]
        public List<ForceDirectedLinkItem> ForceDirectedLinks { get; private set; }

        [JsonPropertyName("nodes")]
        public List<ForceDirectedNodeItem> ForceDirectedNodes { get; private set; }

        public ForceDirectedData()
        {
            ForceDirectedLinks = new List<ForceDirectedLinkItem>();
            ForceDirectedNodes = new List<ForceDirectedNodeItem>();
        }
    }
}
