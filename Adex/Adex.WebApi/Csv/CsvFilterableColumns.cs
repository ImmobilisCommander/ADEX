using System;
using System.Collections.Generic;

namespace Adex.WebApi.Csv
{
    /// <summary>
    /// Columns whose business value is free text and can be searched with a "contains" filter.
    /// Columns holding numbers, dates, codes or identifiers are deliberately left out.
    /// </summary>
    public static class CsvFilterableColumns
    {
        public static readonly IReadOnlySet<string> Names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "adresse",
            "autre_motif",
            "beneficiaire_categorie",
            "beneficiaire_identifiant",
            "beneficiaire_nom_prenom_com_name",
            "beneficiaire_type",
            "convention_liee",
            "dep_name",
            "Département de l'entreprise",
            "entreprise_id",
            "entreprise_nom_pays",
            "id",
            "id_beneficiaire",
            "identifiant_unique",
            "identite",
            "information_evenement",
            "lien_interet",
            "mere_id",
            "motif_lien_interet",
            "Nom Pays (Bénéficiaire)",
            "numero_siren",
            "pays_libelle",
            "prenom",
            "profession_libelle",
            "raison_sociale",
            "reg_name",
            "Région de l'entreprise",
            "secteur_activite",
            "statut",
            "structure_exercice",
            "token",
            "Ville de l'entreprise",
            "ville",
            "ville_sanscedex",
        };
    }
}
