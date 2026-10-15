using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Discovery
{
    public sealed class AuthMdController : Controller
    {
        private const string Document =
            """
            # ADEX auth.md

            ADEX provides no authentication and no agent registration.

            ## Audience

            AI agents, crawlers and automated clients that read ADEX's public data.

            ## Access

            - No authentication is required: no account, API key, token or session.
            - No registration or provisioning endpoint exists; do not attempt `POST /agent/auth`.
            - No OAuth metadata is published (`/.well-known/oauth-protected-resource` and `/.well-known/oauth-authorization-server` are intentionally absent).
            - No credentials are issued or expected: do not send an `Authorization` header.

            ## Public resources

            - HTML pages: `/` and `/{id}` for an entity page.
            - JSON representation of an entity: `/data/entities/{id}.json`.
            - Sitemap: `/sitemap.xml`.
            - Discovery catalog: `/.well-known/api-catalog`.
            - Crawl rules: `/robots.txt`.

            ADEX's internal API is not public; routes under `/api/` are excluded from crawling.

            ## Usage

            Please respect `/robots.txt`, including its `Content-Signal` directive, and keep your request rate reasonable.
            """;

        [HttpGet("/auth.md")]
        public IActionResult Index()
        {
            Response.Headers.CacheControl = "public, max-age=3600";
            return Content(Document, "text/markdown", Encoding.UTF8);
        }
    }
}
