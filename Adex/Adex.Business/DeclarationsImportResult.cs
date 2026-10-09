namespace Adex.Business
{
    public sealed class DeclarationsImportResult
    {
        public long Rows { get; set; }

        public long SkippedRows { get; set; }

        public long Companies { get; set; }

        public long Beneficiaries { get; set; }

        public long Links { get; set; }
    }
}
