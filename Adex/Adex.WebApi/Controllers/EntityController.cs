using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adex.WebApi.Explorer;
using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [ApiController]
    [Route("api/entity")]
    public sealed class EntityController : ControllerBase
    {
        private readonly DataExplorerService _explorer;

        public EntityController(DataExplorerService explorer)
        {
            _explorer = explorer;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<EntitySearchResult>>> Search(
            [FromQuery] string query,
            CancellationToken cancellationToken
        )
        {
            return await _explorer.SearchEntitiesAsync(query, cancellationToken);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EntityDetails>> Get(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var entity = await _explorer.GetEntityAsync(id, cancellationToken);
            return entity is null ? NotFound() : Ok(entity);
        }
    }
}
