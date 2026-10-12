using Adex.Contracts.Explorer;
using Adex.Data.ReadModel.Explorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Entities
{
    /// <summary>
    /// Contrôleur d'accès aux informations d'un bénéficiaire (maintenu pour compatibilité).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiaryController(DataExplorerService explorer) : ControllerBase
    {
        /// <summary>
        /// Récupère la fiche détaillée et les premiers liens d'intérêt d'un bénéficiaire par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique (GUID) du bénéficiaire.</param>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Fiche détaillée du bénéficiaire ou 404 si non trouvé.</returns>
        /// <response code="200">Bénéficiaire trouvé et retourné.</response>
        /// <response code="404">Aucun bénéficiaire correspondant à l'identifiant spécifié.</response>
        [HttpGet]
        [Route("info/{id:guid}")]
        [ProducesResponseType(typeof(EntityDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetInformation(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var entity = await explorer.GetEntityAsync(id, 1, DataExplorerService.DefaultPageSize, "date", true, cancellationToken);
            return entity is null ? NotFound() : Ok(entity);
        }
    }
}
