using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Adex.Mcp.Prompts
{
    [McpServerPromptType]
    public sealed class AdexPrompts
    {
        [McpServerPrompt(Name = "analyser_entite")]
        [Description(
            "Analyse une entité : montants par année, typologies et principales contreparties."
        )]
        public static string AnalyserEntite(
            [Description("Nom (ou référence) de l'entité à analyser.")] string nom
        ) =>
            $"""
            Analyse l'entité « {nom} » avec les outils ADEX :
            1. Appelle `rechercher_entites` avec « {nom} ». S'il y a plusieurs résultats plausibles, liste-les et demande lequel choisir avant de continuer.
            2. Appelle `obtenir_fiche_entite` avec l'identifiant retenu.
            3. Appelle `lister_liens_financiers` avec taillePage=50, tri=amount, decroissant=true.
            Présente : l'identité de l'entité, les montants émis et reçus, l'évolution par année, la répartition par typologie, les principales contreparties et toute concentration inhabituelle (une contrepartie ou une année dominante).
            Base-toi uniquement sur les données retournées par les outils, et précise que les liens listés ne sont qu'une page sur le total.
            """;

        [McpServerPrompt(Name = "comparer_entites")]
        [Description("Compare deux entités : montants, évolution et contreparties.")]
        public static string ComparerEntites(
            [Description("Nom de la première entité.")] string nom1,
            [Description("Nom de la seconde entité.")] string nom2
        ) =>
            $"""
            Compare les entités « {nom1} » et « {nom2} » avec les outils ADEX :
            1. Pour chacune, appelle `rechercher_entites`, puis `obtenir_fiche_entite` avec l'identifiant retenu. En cas d'ambiguïté, demande une précision.
            2. Pour chacune, appelle `lister_liens_financiers` avec taillePage=20, tri=amount, decroissant=true.
            Présente un tableau comparatif (type, montants émis et reçus, nombre de liens, évolution par année), puis les contreparties communes ou distinctes et les différences notables.
            Base-toi uniquement sur les données retournées par les outils.
            """;

        [McpServerPrompt(Name = "synthese_globale")]
        [Description("Synthèse commentée des statistiques globales et du classement.")]
        public static string SyntheseGlobale() =>
            """
            Réalise une synthèse de la base ADEX avec les outils :
            1. Appelle `obtenir_statistiques`.
            2. Appelle `top_entites_par_montant`.
            Présente : le volume (entités, liens, montant total), la répartition par typologie, l'évolution mensuelle des déclarations, le niveau de concentration des montants, puis les principaux contributeurs et bénéficiaires.
            Signale les points notables et rappelle que les chiffres reflètent uniquement les déclarations présentes dans la base.
            """;
    }
}
