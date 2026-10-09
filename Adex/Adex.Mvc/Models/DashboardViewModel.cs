using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Adex.Mvc.Views.Home;

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

        public List<MonthlyDeclarationViewModel> MonthlyDeclarations { get; set; } = new();

        public List<ConcentrationCurveViewModel> Concentration { get; set; } = new();

        public List<AmountHistogramBinViewModel> AmountHistogram { get; set; } = new();

        public DashboardMonthlyChartViewModel MonthlyChart => new(MonthlyDeclarations);

        public AmountHistogramChartViewModel AmountHistogramChart =>
            new(AmountHistogram);

        public ConcentrationChartViewModel ConcentrationChart =>
            new(Concentration);

        public string EntityCountDisplay =>
            EntityCount.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));

        public string FinancialLinkCountDisplay =>
            FinancialLinkCount.ToString("N0", CultureInfo.GetCultureInfo("fr-FR"));

        public string TotalAmountDisplay =>
            TotalAmount.ToString("C0", CultureInfo.GetCultureInfo("fr-FR"));

        public IReadOnlyList<(EntityTypeCountViewModel Entity, int Width)> EntityTypeRows =>
            EntityTypes
                .Select(entity =>
                    (
                        Entity: entity,
                        Width: EntityCount == 0
                            ? 0
                            : (int)Math.Round(entity.Count * 100d / EntityCount)
                    )
                )
                .ToList();

        public IReadOnlyList<(FinancialLinkTypeViewModel Type, int Width)> FinancialLinkTypeRows =>
            FinancialLinkTypes
                .Select(type =>
                    (
                        Type: type,
                        Width: FinancialLinkCount == 0
                            ? 0
                            : (int)Math.Round(type.Count * 100d / FinancialLinkCount)
                    )
                )
                .ToList();

        public RankingModel TopBeneficiariesRanking =>
            new(
                "top-beneficiaries",
                "Top 10 des bénéficiaires par montant reçu",
                TopBeneficiaries
            );

        public RankingModel TopContributorsRanking =>
            new(
                "top-contributors",
                "Top 10 des contributeurs par montant versé",
                TopContributors
            );
    }
}
