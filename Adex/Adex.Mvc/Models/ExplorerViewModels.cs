using System;
using System.Collections.Generic;

namespace Adex.Mvc.Models
{
    public sealed class DashboardViewModel
    {
        public string ErrorMessage { get; set; }

        public int EntityCount { get; set; }

        public int FinancialLinkCount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<EntityTypeCountViewModel> EntityTypes { get; set; } = new();

        public List<FinancialLinkTypeViewModel> FinancialLinkTypes { get; set; } = new();

        public List<TopEntityViewModel> TopContributors { get; set; } = new();

        public List<TopEntityViewModel> TopBeneficiaries { get; set; } = new();
    }

    public sealed class EntityTypeCountViewModel
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public sealed class FinancialLinkTypeViewModel
    {
        public string Type { get; set; } = string.Empty;

        public int Count { get; set; }

        public decimal Amount { get; set; }
    }

    public sealed class TopEntityViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public sealed class EntitySearchResultViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }

    public sealed class EntitySearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string ErrorMessage { get; set; }

        public List<EntitySearchResultViewModel> Results { get; set; } = new();
    }

    public sealed class EntityDetailsViewModel
    {
        public string ErrorMessage { get; set; }

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

        public List<EntityAttributeViewModel> Attributes { get; set; } = new();

        public List<FinancialLinkViewModel> FinancialLinks { get; set; } = new();
    }

    public sealed class EntityAttributeViewModel
    {
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public sealed class FinancialLinkViewModel
    {
        public Guid Id { get; set; }

        public Guid CounterpartyId { get; set; }

        public string CounterpartyName { get; set; } = string.Empty;

        public string DeclarationType { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public bool Outgoing { get; set; }

        public List<EntityAttributeViewModel> Justification { get; set; } = new();
    }
}
