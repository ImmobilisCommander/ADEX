using Adex.Application.Import;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Import
{
    /// <summary>
    /// Contrôleur d'administration et de pilotage du travail d'importation des déclarations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController(ImportJobService importJobService) : ControllerBase
    {
        private const string ApiKeyHeader = "X-Api-Key";

        /// <summary>
        /// Déclenche l'exécution en arrière-plan d'un import complet du fichier CSV des déclarations.
        /// </summary>
        /// <param name="apiKey">Clé d'administration transmise dans l'en-tête « X-Api-Key ».</param>
        /// <returns>Statut du travail d'import démarré.</returns>
        /// <response code="202">L'import a été démarré avec succès.</response>
        /// <response code="401">Clé d'API manquante ou non autorisée.</response>
        /// <response code="404">Le service d'import n'est pas activé.</response>
        /// <response code="409">Un import est déjà en cours d'exécution.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ImportStatus), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ImportStatus), StatusCodes.Status409Conflict)]
        public ActionResult<ImportStatus> Start([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            if (Check(apiKey) is { } denied)
            {
                return denied;
            }

            return importJobService.TryStart(out var status)
                ? AcceptedAtAction(nameof(GetStatus), status)
                : Conflict(status);
        }

        /// <summary>
        /// Consulte l'état courant et l'avancement de l'import des déclarations.
        /// </summary>
        /// <param name="apiKey">Clé d'administration transmise dans l'en-tête « X-Api-Key ».</param>
        /// <returns>Statut détaillé de l'import (état, étape en cours, compteurs et messages d'erreur éventuels).</returns>
        /// <response code="200">Statut de l'import récupéré avec succès.</response>
        /// <response code="401">Clé d'API manquante ou non autorisée.</response>
        /// <response code="404">Le service d'import n'est pas activé.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ImportStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ImportStatus> GetStatus([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            return Check(apiKey) as ActionResult ?? Ok(importJobService.GetStatus());
        }

        /// <summary>
        /// Demande l'annulation d'un import en cours d'exécution.
        /// </summary>
        /// <param name="apiKey">Clé d'administration transmise dans l'en-tête « X-Api-Key ».</param>
        /// <returns>Confirmation de la demande d'annulation.</returns>
        /// <response code="202">Demande d'annulation acceptée.</response>
        /// <response code="204">Aucun import n'est en cours d'exécution.</response>
        /// <response code="401">Clé d'API manquante ou non autorisée.</response>
        /// <response code="404">Le service d'import n'est pas activé.</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Cancel([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            return Check(apiKey) ?? (importJobService.Cancel() ? Accepted() : NoContent());
        }

        private ActionResult Check(string apiKey)
        {
            if (!importJobService.IsEnabled)
            {
                return NotFound();
            }

            return importJobService.IsAuthorized(apiKey) ? null : Unauthorized();
        }
    }
}
