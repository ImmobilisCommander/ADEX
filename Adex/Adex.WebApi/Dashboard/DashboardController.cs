using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Contracts.Explorer;
using Adex.Data.ReadModel.Explorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adex.WebApi.Dashboard
{
    /// <summary>
    /// Contrôleur fournissant les statistiques, indicateurs clés et graphiques du tableau de bord.
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    public sealed class DashboardController(DataExplorerService explorer) : ControllerBase
    {
        /// <summary>
        /// Récupère le snapshot complet des indicateurs et graphiques du tableau de bord.
        /// </summary>
        /// <remarks>
        /// Cette opération est bloquante si le snapshot est en cours de génération ou expiré.
        /// </remarks>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Le modèle complet du tableau de bord (statistiques globales, graphiques et classements).</returns>
        /// <response code="200">Tableau de bord retourné avec succès.</response>
        /// <response code="503">Un import est en cours, les données sont indisponibles.</response>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DashboardModel>> Get(CancellationToken cancellationToken)
        {
            return await explorer.GetDashboardAsync(cancellationToken);
        }

        /// <summary>
        /// Récupère le snapshot du tableau de bord de manière progressive et non bloquante.
        /// </summary>
        /// <remarks>
        /// Si le snapshot n'est pas encore prêt ou est expiré, un calcul en arrière-plan est initié et le code HTTP 202 Accepted est renvoyé.
        /// </remarks>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Le modèle du tableau de bord s'il est prêt, sinon un statut 202 Accepted.</returns>
        /// <response code="200">Le tableau de bord est prêt et retourné.</response>
        /// <response code="202">Le calcul du tableau de bord est en cours en arrière-plan.</response>
        /// <response code="503">Un import est en cours, les données sont indisponibles.</response>
        [HttpGet("progressive")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [ProducesResponseType(typeof(DashboardModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DashboardModel>> GetProgressive(CancellationToken cancellationToken)
        {
            var dashboard = await explorer.TryGetDashboardAsync(cancellationToken);
            return dashboard is null ? Accepted() : dashboard;
        }
    }
}
