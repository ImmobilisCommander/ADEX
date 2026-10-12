using System;
using System.Collections.Generic;
using System.Linq;

namespace Adex.Mvc.Entities
{
    public sealed class EntitySearchPageViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string ErrorMessage { get; set; }

        public List<EntitySearchResultViewModel> Results { get; set; } = new();

        public bool HasQuery => !string.IsNullOrWhiteSpace(Query);

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool HasResults => Results.Count > 0;

        public string QueryDisplay => HasQuery ? $"« {Query} »" : string.Empty;

        public string PageTitle => HasQuery ? "Résultats de recherche" : "Rechercher une entité";

        public string HeadingTitle => HasQuery ? "Résultats de recherche" : "Rechercher une entité";

        public string EmptyStateMessage => HasQuery
            ? "Aucune entité ne correspond à cette recherche. Essayez un nom ou un identifiant différent."
            : "Saisissez le nom d’une entreprise, d’un bénéficiaire ou un identifiant pour lancer une recherche.";

        public string EntityUrl(EntitySearchResultViewModel result) =>
            HasQuery
                ? $"/{result.Id}?fromQuery={Uri.EscapeDataString(Query)}"
                : $"/{result.Id}";

        // Attribute columns present in the results, "Référence" excluded since it is shown with the name.
        public IReadOnlyList<string> AttributeColumns =>
            Results
                .SelectMany(result => result.Attributes.Keys)
                .Distinct()
                .OrderBy(name =>
                    Array.IndexOf(PreferredColumns, name) is var index && index >= 0
                        ? index
                        : int.MaxValue
                )
                .ThenBy(name => name, StringComparer.CurrentCulture)
                .ToList();

        private static readonly string[] PreferredColumns =
        {
            "Référence",
            "Identifiant",
            "Type d'identifiant",
            "Catégorie",
            "Profession",
            "Structure d'exercice",
            "Adresse",
            "Code postal",
            "Ville",
            "Pays",
            "SIREN",
            "Secteur d'activité",
            "Société mère",
            "Département",
            "Région",
        };
    }
}
