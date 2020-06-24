// <copyright file="GraphDataSet.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.Collections.Generic;

namespace Adex.Common
{
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
