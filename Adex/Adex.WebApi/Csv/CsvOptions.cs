using System.ComponentModel.DataAnnotations;

namespace Adex.WebApi.Csv
{
    public sealed class CsvOptions
    {
        public const string SectionName = "Csv";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le paramètre « Csv:FilePath » est obligatoire.")]
        public string FilePath { get; set; } = string.Empty;

        [Range(0, 86400, ErrorMessage = "Le paramètre « Csv:CacheDurationSeconds » doit être compris entre 0 et 86400.")]
        public int CacheDurationSeconds { get; set; } = 60;
    }
}
