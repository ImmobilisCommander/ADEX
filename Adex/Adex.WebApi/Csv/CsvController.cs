using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.Application.Csv;
using Adex.Contracts.Csv;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Csv
{
    /// <summary>
    /// Contrôleur d'interrogation et de pagination du fichier CSV consolidé.
    /// </summary>
    [ApiController]
    [Route("api/csv")]
    public sealed class CsvController(ICsvService csvService) : ControllerBase
    {
        /// <summary>
        /// Récupère une page de déclarations issues du fichier CSV avec filtres textuels optionnels.
        /// </summary>
        /// <param name="page">Numéro de page à récupérer (commençant à 1).</param>
        /// <param name="filters">Dictionnaire de filtres par colonne (clé = nom de colonne, valeur = texte recherché).</param>
        /// <param name="cancellationToken">Jeton d'annulation de la requête HTTP.</param>
        /// <returns>Une page de données CSV contenant les colonnes, les lignes et l'indicateur de page suivante.</returns>
        /// <response code="200">Page CSV récupérée avec succès.</response>
        /// <response code="400">Un ou plusieurs filtres demandés ne sont pas autorisés.</response>
        /// <response code="404">Le fichier CSV est introuvable ou inaccessible.</response>
        [HttpGet]
        [ProducesResponseType(typeof(CsvPage), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CsvPage>> Get(
            [FromQuery] int page = 1,
            [FromQuery(Name = "filters")] Dictionary<string, string> filters = null,
            CancellationToken cancellationToken = default
        )
        {
            return await csvService.GetPageAsync(page, filters, cancellationToken);
        }
    }
}
