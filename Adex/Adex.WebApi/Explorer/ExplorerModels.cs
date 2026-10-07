using System;
using System.Collections.Generic;

namespace Adex.WebApi.Explorer
{
    public sealed class DashboardModel
    {
        public int EntityCount { get; set; }

        public int FinancialLinkCount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<EntityTypeCount> EntityTypes { get; set; } = new();

        public List<FinancialLinkTypeSummary> FinancialLinkTypes { get; set; } = new();

        public List<TopEntity> TopContributors { get; set; } = new();

        public List<TopEntity> TopBeneficiaries { get; set; } = new();
    }

    public sealed class EntityTypeCount
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public sealed class FinancialLinkTypeSummary
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class TopEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public sealed class EntitySearchResult
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }

    public sealed class EntityDetails
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal OutgoingAmount { get; set; }

        public decimal IncomingAmount { get; set; }

        public int FinancialLinkCount { get; set; }

        public bool FinancialLinksTruncated { get; set; }

        public List<EntityAttributeDetails> Attributes { get; set; } = new();

        public List<FinancialLinkDetails> FinancialLinks { get; set; } = new();
    }

    public sealed class EntityAttributeDetails
    {
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public sealed class FinancialLinkDetails
    {
        public Guid Id { get; set; }

        public Guid CounterpartyId { get; set; }

        public string CounterpartyName { get; set; } = string.Empty;

        public string DeclarationType { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public bool Outgoing { get; set; }

        public List<EntityAttributeDetails> Justification { get; set; } = new();
    }
}
