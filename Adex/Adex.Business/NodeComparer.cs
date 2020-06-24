// <copyright file="CvsLoaderMetadata.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Adex.Business
{
    internal class NodeComparer : IEqualityComparer<ForceDirectedNodeItem>
    {
        public bool Equals([AllowNull] ForceDirectedNodeItem x, [AllowNull] ForceDirectedNodeItem y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] ForceDirectedNodeItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
