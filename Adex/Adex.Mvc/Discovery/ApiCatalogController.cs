using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Adex.Mvc.Discovery
{
    public sealed class ApiCatalogController : Controller
    {
        public const string ApiCatalogPath = "/.well-known/api-catalog";

        public const string ServiceDocPath = "/Home/Disclaimer";

        public const string EntityDataPath = "/data/entities/";

        [HttpGet(ApiCatalogPath)]
        public IActionResult Index()
        {
            var catalog = new Dictionary<string, object>
            {
                ["linkset"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["anchor"] = EntityDataPath,
                        ["service-doc"] = new[]
                        {
                            new Dictionary<string, string>
                            {
                                ["href"] = ServiceDocPath,
                                ["type"] = "text/html",
                            },
                        },
                        ["describedby"] = new[]
                        {
                            new Dictionary<string, string>
                            {
                                ["href"] = "/sitemap.xml",
                                ["type"] = "application/xml",
                            },
                        },
                    },
                },
            };

            Response.Headers.CacheControl = "public, max-age=3600";
            return Content(
                JsonSerializer.Serialize(catalog),
                "application/linkset+json; profile=\"https://www.rfc-editor.org/info/rfc9727\""
            );
        }
    }
}
