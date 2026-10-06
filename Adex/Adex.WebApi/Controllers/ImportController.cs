using Adex.WebApi.Import;

using Microsoft.AspNetCore.Mvc;

namespace Adex.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private const string ApiKeyHeader = "X-Api-Key";
        private readonly ImportJobService _importJobService;

        public ImportController(ImportJobService importJobService)
        {
            _importJobService = importJobService;
        }

        [HttpPost]
        public ActionResult<ImportStatus> Start(
            [FromHeader(Name = ApiKeyHeader)] string apiKey,
            [FromQuery] ImportTarget target = ImportTarget.All
        )
        {
            if (Check(apiKey) is { } denied)
            {
                return denied;
            }

            return _importJobService.TryStart(target, out var status)
                ? AcceptedAtAction(nameof(GetStatus), status)
                : Conflict(status);
        }

        [HttpGet]
        public ActionResult<ImportStatus> GetStatus([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            return Check(apiKey) as ActionResult ?? Ok(_importJobService.GetStatus());
        }

        [HttpDelete]
        public ActionResult Cancel([FromHeader(Name = ApiKeyHeader)] string apiKey)
        {
            return Check(apiKey) ?? (_importJobService.Cancel() ? Accepted() : NoContent());
        }

        private ActionResult Check(string apiKey)
        {
            if (!_importJobService.IsEnabled)
            {
                return NotFound();
            }

            return _importJobService.IsAuthorized(apiKey) ? null : Unauthorized();
        }
    }
}
