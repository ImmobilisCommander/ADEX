using Adex.WebApi.Import;

using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;

namespace Adex.WebApi.Hosting
{
    public class ImportInProgressMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ImportJobService importJob)
        {
            if (
                !context.Request.Path.StartsWithSegments("/api/import")
                && !context.Request.Path.StartsWithSegments("/swagger")
                && importJob.IsRunning
            )
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.Headers.RetryAfter = "30";
                await context.Response.WriteAsJsonAsync(
                    new { error = "Import en cours, les données sont indisponibles." }
                );
                return;
            }

            await next(context);
        }
    }
}
