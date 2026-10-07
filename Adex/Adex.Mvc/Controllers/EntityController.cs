using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Adex.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Adex.Mvc.Controllers
{
    public sealed class EntityController : Controller
    {
        private readonly AdexApiClient _apiClient;
        private readonly ILogger<EntityController> _logger;

        public EntityController(AdexApiClient apiClient, ILogger<EntityController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet("/{reference:nonfile}")]
        public async Task<IActionResult> Details(
            string reference,
            CancellationToken cancellationToken
        )
        {
            EntityDetailsViewModel entity;
            try
            {
                entity = await _apiClient.GetEntityAsync(reference, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load entity details for {EntityReference}",
                    reference
                );
                return View(
                    new EntityDetailsViewModel
                    {
                        Reference = reference,
                        Name = reference,
                        ErrorMessage =
                            "Les données de cette entité ne sont pas accessibles. Réessayez dans un instant."
                    }
                );
            }

            if (entity is null)
            {
                Response.StatusCode = 404;
                return View("NotFound", reference);
            }

            return View(entity);
        }
    }
}
