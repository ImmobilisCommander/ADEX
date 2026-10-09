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
}
