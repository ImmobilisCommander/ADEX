using System;
using System.Collections.Generic;
using System.Linq;

namespace Adex.Mvc.Models
{

    public sealed class EntitySearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string ErrorMessage { get; set; }

        public List<EntitySearchResultViewModel> Results { get; set; } = new();

        // Attribute columns present in the results, "Référence" excluded since it is shown with the name.
        public IReadOnlyList<string> AttributeColumns =>
            Results
                .SelectMany(result => result.Attributes.Keys)
                .Distinct()
                .OrderBy(name => Array.IndexOf(PreferredColumns, name) is var index && index >= 0 ? index : int.MaxValue)
                .ThenBy(name => name, StringComparer.CurrentCulture)
                .ToList();

        private static readonly string[] PreferredColumns =
        {
            "Référence", "Identifiant", "Type d'identifiant", "Catégorie", "Profession",
            "Structure d'exercice", "Adresse", "Code postal", "Ville", "Pays",
            "SIREN", "Secteur d'activité", "Société mère", "Département", "Région",
        };
    }
}
