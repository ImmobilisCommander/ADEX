using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{

    public sealed class EntitySearchResult
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }
}
