using System.Collections.Generic;
using System.Linq;

namespace Adex.Mvc.Models
{
    public sealed class CsvIndexViewModel
    {
        public string Title { get; } = "Interroger le CSV";

        public int PageSize { get; init; } = 10;

        public string ContentUrl { get; init; }

        public IReadOnlyList<int> PlaceholderRows { get; } = Enumerable.Range(0, 10).ToList();

        public IReadOnlyList<int> PlaceholderColumns { get; } = Enumerable.Range(0, 8).ToList();
    }
}
