using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Entities.Json
{
    [ApiController]
    public sealed class EntityJsonController(EntityDetailsCache entityDetailsCache) : ControllerBase
    {
        [HttpGet("/data/entities/{id:guid}.json", Name = "entity-json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(EntityJsonDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EntityJsonDto>> Get(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            var entity = await entityDetailsCache.GetAsync(
                id,
                page,
                pageSize,
                sort ?? "date",
                descending,
                cancellationToken
            );

            if (entity is null)
            {
                return NotFound();
            }

            return Ok(EntityJsonDto.FromEntity(entity));
        }
    }
}
