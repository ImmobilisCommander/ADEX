using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Mvc.Api;
using Adex.Mvc.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Entities
{
    public sealed class EntityController(AdexApiClient apiClient) : Controller
    {
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
            var entity = await apiClient.GetEntityAsync(id, page, pageSize, sort ?? "date", descending, cancellationToken);
            if (entity is null)
            {
                ViewData["Title"] = "Entité introuvable";
                Response.StatusCode = 404;
                return View("NotFound", new NotFoundViewModel { Identifier = id.ToString() });
            }

            ViewData["Title"] = entity.Name;
            return View(entity);
        }
    }
}
