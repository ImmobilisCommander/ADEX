using System.Collections.Generic;

namespace Adex.WebApi.Csv
{
    public sealed class CsvPage
    {
        public IReadOnlyList<string> Columns { get; init; } = new List<string>();

        public IReadOnlyList<string> FilterableColumns { get; init; } = new List<string>();

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = new List<IReadOnlyList<string>>();

        public bool HasNextPage { get; init; }
    }
}
