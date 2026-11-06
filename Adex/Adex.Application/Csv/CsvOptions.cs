using System.ComponentModel.DataAnnotations;

namespace Adex.Application.Csv
{
    public sealed class CsvOptions
    {
        public const string SectionName = "Csv";

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Le paramètre « Csv:FilePath » est obligatoire."
        )]
        public required string FilePath { get; set; }

        [Range(
            0,
            86400,
            ErrorMessage = "Le paramètre « Csv:CacheDurationSeconds » doit être compris entre 0 et 86400."
        )]
        public required int CacheDurationSeconds { get; set; }
    }
}
