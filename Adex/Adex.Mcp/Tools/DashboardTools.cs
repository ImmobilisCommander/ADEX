using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.WebApi.Client;
using ModelContextProtocol.Server;

namespace Adex.Mcp.Tools
{
    [McpServerToolType]
    public sealed class DashboardTools(IAdexWebApiClient client)
    {
        [McpServerTool(Name = "obtenir_statistiques", ReadOnly = true)]
        [Description(
            "Statistiques globales : nombre d'entités et de liens, montant total, montants par typologie, déclarations par mois, concentration et distribution des montants."
        )]
        public async Task<DashboardModel> ObtenirStatistiquesAsync(
            CancellationToken cancellationToken
        )
        {
            var dashboard = await client.GetDashboardAsync(cancellationToken);
            dashboard.TopContributors = [];
            dashboard.TopBeneficiaries = [];
            return dashboard;
        }

        [McpServerTool(Name = "top_entites_par_montant", ReadOnly = true)]
        [Description(
            "Classement des entités par montant financier cumulé : principaux contributeurs et principaux bénéficiaires."
        )]
        public async Task<TopEntitiesResult> TopEntitesParMontantAsync(
            CancellationToken cancellationToken
        )
        {
            var dashboard = await client.GetDashboardAsync(cancellationToken);
            return new TopEntitiesResult(dashboard.TopContributors, dashboard.TopBeneficiaries);
        }
    }
}
