using System.ComponentModel.DataAnnotations;

namespace Adex.WebApi.Import
{
    public class ImportOptions
    {
        public const string SectionName = "Import";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le paramètre « Import:DataDirectory » est obligatoire.")]
        public string DataDirectory { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Le paramètre « Import:FileName » est obligatoire.")]
        public string FileName { get; set; } = "declarations.csv";

        // Une clé vide désactive volontairement l'API d'import.
        public string ApiKey { get; set; } = string.Empty;
    }
}
