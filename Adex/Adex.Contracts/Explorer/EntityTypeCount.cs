using System;
using System.Collections.Generic;

namespace Adex.Contracts.Explorer
{
    public sealed class EntityTypeCount
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
