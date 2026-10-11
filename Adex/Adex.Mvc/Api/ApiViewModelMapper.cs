using System.Linq;
using Adex.Contracts.Csv;
using Adex.Contracts.Explorer;
using Adex.Mvc.Csv;
using Adex.Mvc.Entities;
using Adex.Mvc.Home;

namespace Adex.Mvc.Api
{
    internal static class ApiViewModelMapper
    {
        public static DashboardViewModel ToViewModel(DashboardModel source)
        {
            return new DashboardViewModel
            {
                EntityCount = source.EntityCount,
                FinancialLinkCount = source.FinancialLinkCount,
                TotalAmount = source.TotalAmount,
                EntityTypes = source.EntityTypes.Select(item => new EntityTypeCountViewModel
                {
                    Type = item.Type,
                    Count = item.Count,
                }).ToList(),
                FinancialLinkTypes = source.FinancialLinkTypes.Select(item => new FinancialLinkTypeViewModel
                {
                    Type = item.Type,
                    Count = item.Count,
                    Amount = item.Amount,
                }).ToList(),
                TopContributors = source.TopContributors.Select(ToViewModel).ToList(),
                TopBeneficiaries = source.TopBeneficiaries.Select(ToViewModel).ToList(),
                MonthlyDeclarations = source.MonthlyDeclarations.Select(item => new MonthlyDeclarationViewModel
                {
                    Year = item.Year,
                    Month = item.Month,
                    Count = item.Count,
                }).ToList(),
                Concentration = source.Concentration.Select(item => new ConcentrationCurveViewModel
                {
                    Label = item.Label,
                    Count = item.Count,
                    Total = item.Total,
                    TopOnePercentShare = item.TopOnePercentShare,
                    TopTenShare = item.TopTenShare,
                    Points = item.Points.Select(point => new ConcentrationPointViewModel
                    {
                        PopulationPercent = point.PopulationPercent,
                        AmountPercent = point.AmountPercent,
                    }).ToList(),
                }).ToList(),
                AmountHistogram = source.AmountHistogram.Select(item => new AmountHistogramBinViewModel
                {
                    LowerBound = item.LowerBound,
                    UpperBound = item.UpperBound,
                    Count = item.Count,
                }).ToList(),
            };
        }

        public static EntitySearchResultViewModel ToViewModel(EntitySearchResult source)
        {
            return new EntitySearchResultViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Type = source.Type,
                Detail = source.Detail,
                LinkCount = source.LinkCount,
                Amount = source.Amount,
                Attributes = source.Attributes,
            };
        }

        public static EntityDetailsViewModel ToViewModel(EntityDetails source)
        {
            return new EntityDetailsViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Type = source.Type,
                OutgoingAmount = source.OutgoingAmount,
                IncomingAmount = source.IncomingAmount,
                FinancialLinkCount = source.FinancialLinkCount,
                Page = source.Page,
                Sort = source.Sort,
                Descending = source.Descending,
                PageSize = source.PageSize,
                PageCount = source.PageCount,
                YearlyActivity = source.YearlyActivity.Select(item => new EntityYearActivityViewModel
                {
                    Year = item.Year,
                    Count = item.Count,
                    Amount = item.Amount,
                }).ToList(),
                TypeBreakdown = source.TypeBreakdown.Select(item => new EntityTypeBreakdownViewModel
                {
                    Type = item.Type,
                    Count = item.Count,
                    Amount = item.Amount,
                }).ToList(),
                Attributes = source.Attributes.Select(ToViewModel).ToList(),
                FinancialLinks = source.FinancialLinks.Select(item => new FinancialLinkViewModel
                {
                    Id = item.Id,
                    CounterpartyId = item.CounterpartyId,
                    CounterpartyName = item.CounterpartyName,
                    DeclarationType = item.DeclarationType,
                    Kind = item.Kind,
                    Date = item.Date,
                    Amount = item.Amount,
                    Outgoing = item.Outgoing,
                    Justification = item.Justification.Select(ToViewModel).ToList(),
                }).ToList(),
            };
        }

        public static CsvPageResponse ToViewModel(CsvPage source)
        {
            return new CsvPageResponse
            {
                Columns = source.Columns,
                FilterableColumns = source.FilterableColumns,
                Rows = source.Rows,
                HasNextPage = source.HasNextPage,
            };
        }

        private static TopEntityViewModel ToViewModel(TopEntity source)
        {
            return new TopEntityViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Type = source.Type,
                Amount = source.Amount,
            };
        }

        private static EntityAttributeViewModel ToViewModel(EntityAttributeDetails source)
        {
            return new EntityAttributeViewModel
            {
                Name = source.Name,
                Value = source.Value,
            };
        }
    }
}
