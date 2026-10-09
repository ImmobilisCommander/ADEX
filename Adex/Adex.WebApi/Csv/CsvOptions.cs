using System.ComponentModel.DataAnnotations;

namespace Adex.WebApi.Csv
{
    public sealed class CsvOptions
    {
        public const string SectionName = "Csv";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le paramètre « Csv:FilePath » est obligatoire.")]
        public string FilePath { get; set; } = string.Empty;
    }
}
