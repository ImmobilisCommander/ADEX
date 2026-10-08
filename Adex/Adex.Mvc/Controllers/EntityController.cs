using System;
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

        [HttpGet("/{id:guid}")]
        public async Task<IActionResult> Details(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            EntityDetailsViewModel entity;
            try
            {
                entity = await _apiClient.GetEntityAsync(id, page, pageSize, sort ?? "date", descending, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load entity details for {EntityId}",
                    id
                );
                return View(
                    new EntityDetailsViewModel
                    {
                        Id = id,
                        Name = "(entité inconnue)",
                        ErrorMessage =
                            "Les données de cette entité ne sont pas accessibles. Réessayez dans un instant."
                    }
                );
            }

            if (entity is null)
            {
                Response.StatusCode = 404;
                return View("NotFound", id.ToString());
            }

            return View(entity);
        }
    }
}
