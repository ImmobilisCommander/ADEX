using Microsoft.AspNetCore.Builder;

namespace Adex.Mcp.Hosting
{
    public static class WebApplicationExtensions
    {
        public static WebApplication MapAdexEndpoints(this WebApplication app)
        {
            app.MapMcp("/mcp");
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready");
            return app;
        }
    }
}
