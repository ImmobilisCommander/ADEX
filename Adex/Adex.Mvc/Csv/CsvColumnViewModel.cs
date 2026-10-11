namespace Adex.Mvc.Csv
{
    public sealed class CsvColumnViewModel
    {
        public string Name { get; init; } = string.Empty;

        public bool IsFilterable { get; init; }

        public string FilterValue { get; init; } = string.Empty;
    }
}
