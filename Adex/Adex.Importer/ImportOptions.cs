using System.ComponentModel.DataAnnotations;

namespace Adex.Importer
{
    public class ImportOptions
    {
        public const string SectionName = "Import";

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Le paramètre « Import:DataDirectory » est obligatoire."
        )]
        public required string DataDirectory { get; set; }

        [Required(
            AllowEmptyStrings = false,
            ErrorMessage = "Le paramètre « Import:FileName » est obligatoire."
        )]
        public required string FileName { get; set; }
    }
}
