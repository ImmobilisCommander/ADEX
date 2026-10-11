using Adex.Contracts.Explorer;
using Adex.Data.ReadModel.Explorer;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Entities
{
    [ApiController]
    [Route("api/entity")]
    public sealed class EntityController(DataExplorerService explorer) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<ActionResult<List<EntitySearchResult>>> Search(
            [FromQuery] string query,
            CancellationToken cancellationToken
        )
        {
            return await explorer.SearchEntitiesAsync(query, cancellationToken);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EntityDetails>> Get(
            Guid id,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DataExplorerService.DefaultPageSize,
            [FromQuery] string sort = "date",
            [FromQuery] bool descending = true
        )
        {
            var entity = await explorer.GetEntityAsync(id, page, pageSize, sort, descending, cancellationToken);
            return entity is null ? NotFound() : Ok(entity);
        }
    }
}
