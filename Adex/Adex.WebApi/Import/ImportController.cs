using Adex.Application.Import;

using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Import
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController(ImportJobService importJobService) : ControllerBase
    {
        private const string ApiKeyHeader = "X-Api-Key";

        [HttpPost]
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

        [HttpGet]
        public ActionResult<ImportStatus> GetStatus([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            return Check(apiKey) as ActionResult ?? Ok(importJobService.GetStatus());
        }

        [HttpDelete]
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
