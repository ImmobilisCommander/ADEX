using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{

    public sealed class TopEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
