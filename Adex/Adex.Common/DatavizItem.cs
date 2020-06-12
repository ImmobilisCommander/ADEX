// <copyright file="DatavizItem.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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

    public class ForceDirectedLinkItem
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }
    }

    public class ForceDirectedNodeItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("group")]
        public int Group { get; set; }
    }

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

    public class GraphDataSet
    {
        public ForceDirectedData ForceDirectedData { get; set; }

        public List<EdgeBundlingItem> BundlingItems { get; private set; }

        public GraphDataSet()
        {
            BundlingItems = new List<EdgeBundlingItem>();
        }
    }
}
