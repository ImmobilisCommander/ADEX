using System.Collections.Generic;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class CsvPageViewModel
    {
        public string Title { get; } = "Interroger le CSV";

        public string ErrorMessage { get; init; }

        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 10;

        public IReadOnlyList<CsvColumnViewModel> Columns { get; init; } = new List<CsvColumnViewModel>();

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = new List<IReadOnlyList<string>>();

        public string PreviousPageUrl { get; init; }

        public string NextPageUrl { get; init; }

        public string ClearFiltersUrl { get; init; }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool HasRows => Rows.Count > 0;

        public bool HasPager => PreviousPageUrl is not null || NextPageUrl is not null;

        public bool HasActiveFilters => Columns.Any(x => x.FilterValue.Length > 0);

        public bool HasFilterableColumns => Columns.Any(x => x.IsFilterable);

        public int FirstRowNumber => (Page - 1) * PageSize + 1;

        public int LastRowNumber => FirstRowNumber + Rows.Count - 1;
    }
}
