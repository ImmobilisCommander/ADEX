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

        public List<MonthlyDeclarationCount> MonthlyDeclarations { get; set; } = new();

        public List<ConcentrationCurve> Concentration { get; set; } = new();

        public List<AmountHistogramBin> AmountHistogram { get; set; } = new();
    }

    public sealed class ConcentrationCurve
    {
        public string Label { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Total { get; set; }

        public double TopOnePercentShare { get; set; }

        public double TopTenShare { get; set; }

        public List<ConcentrationPoint> Points { get; set; } = new();
    }

    // Part cumulée du montant (en %) détenue par les premiers x % de la population classée par montant décroissant.
    public sealed class ConcentrationPoint
    {
        public double PopulationPercent { get; set; }

        public double AmountPercent { get; set; }
    }

    // Tranche d'une demi-décade : [LowerBound, UpperBound[. Les montants nuls ou négatifs ont LowerBound = UpperBound = 0.
    public sealed class AmountHistogramBin
    {
        public decimal LowerBound { get; set; }

        public decimal UpperBound { get; set; }

        public int Count { get; set; }
    }

    public sealed class EntityYearActivity
    {
        public int Year { get; set; }

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class EntityTypeBreakdown
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class MonthlyDeclarationCount
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Count { get; set; }
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

        internal string SortName { get; set; }

        public List<EntityAttributeDetails> Justification { get; set; } = new();
    }
}
