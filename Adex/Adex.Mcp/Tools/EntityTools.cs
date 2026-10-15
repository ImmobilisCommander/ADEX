using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.WebApi.Client;
using ModelContextProtocol.Server;

namespace Adex.Mcp.Tools
{
    [McpServerToolType]
    public sealed class EntityTools(IAdexWebApiClient client)
    {
        [McpServerTool(Name = "rechercher_entites", ReadOnly = true)]
        [Description(
            "Recherche des entités (entreprises, bénéficiaires) par nom ou référence. Retourne leur identifiant, type, nombre de liens et montant."
        )]
        public Task<IReadOnlyList<EntitySearchResult>> RechercherEntitesAsync(
            [Description("Texte recherché (nom ou référence).")] string requete,
            CancellationToken cancellationToken
        ) => client.SearchEntitiesAsync(requete, cancellationToken);

        [McpServerTool(Name = "obtenir_fiche_entite", ReadOnly = true)]
        [Description(
            "Retourne la fiche d'une entité : attributs, montants émis/reçus, activité par année et répartition des liens par typologie (sans la liste des liens)."
        )]
        public async Task<EntityDetails> ObtenirFicheEntiteAsync(
            [Description("Identifiant (GUID) de l'entité.")] Guid id,
            CancellationToken cancellationToken
        )
        {
            var details = await client.GetEntityAsync(
                id,
                page: 1,
                pageSize: 10,
                cancellationToken: cancellationToken
            );
            details.FinancialLinks = [];
            return details;
        }

        [McpServerTool(Name = "lister_liens_financiers", ReadOnly = true)]
        [Description(
            "Liste paginée des liens financiers d'une entité (contreparties, montants, dates, typologie)."
        )]
        public async Task<FinancialLinksPage> ListerLiensFinanciersAsync(
            [Description("Identifiant (GUID) de l'entité.")] Guid id,
            CancellationToken cancellationToken,
            [Description("Numéro de page, à partir de 1.")] int page = 1,
            [Description("Taille de page : 10, 20 ou 50 (toute autre valeur vaut 20).")]
                int taillePage = 20,
            [Description("Clé de tri : date, amount, type, kind, direction ou name.")]
                string tri = "date",
            [Description("Tri décroissant si vrai.")] bool decroissant = true
        )
        {
            var details = await client.GetEntityAsync(
                id,
                Math.Max(1, page),
                taillePage,
                tri,
                decroissant,
                cancellationToken
            );
            return new FinancialLinksPage(
                details.Id,
                details.Name,
                details.FinancialLinkCount,
                details.Page,
                details.PageCount,
                details.FinancialLinks
            );
        }
    }
}
