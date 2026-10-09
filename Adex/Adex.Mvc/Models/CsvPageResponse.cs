using System.Collections.Generic;

namespace Adex.Mvc.Models
{
    public sealed class CsvPageResponse
    {
        public string ErrorMessage { get; init; }

        public IReadOnlyList<string> Columns { get; init; } = new List<string>();

        public IReadOnlyList<string> FilterableColumns { get; init; } = new List<string>();

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = new List<IReadOnlyList<string>>();

        public bool HasNextPage { get; init; }
    }
}
