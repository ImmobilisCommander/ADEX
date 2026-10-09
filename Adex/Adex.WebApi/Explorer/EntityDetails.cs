using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{

    public sealed class EntityDetails
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal OutgoingAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public int FinancialLinkCount { get; set; }

        public int Page { get; set; } = 1;

        public string Sort { get; set; } = "date";

        public bool Descending { get; set; } = true;

        public int PageSize { get; set; }

        public int PageCount { get; set; } = 1;

        public List<EntityYearActivity> YearlyActivity { get; set; } = new();

        public List<EntityTypeBreakdown> TypeBreakdown { get; set; } = new();

        public List<EntityAttributeDetails> Attributes { get; set; } = new();

        public List<FinancialLinkDetails> FinancialLinks { get; set; } = new();
    }
}
