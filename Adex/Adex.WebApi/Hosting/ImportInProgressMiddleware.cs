using System;
using System.Threading.Tasks;
using Adex.Application.Import;
using Microsoft.AspNetCore.Http;

namespace Adex.WebApi.Hosting
{
    public class ImportInProgressMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ImportJobService importJob)
        {
            if (
                !context.Request.Path.StartsWithSegments(
                    "/api/import",
                    StringComparison.OrdinalIgnoreCase
                )
                && !context.Request.Path.StartsWithSegments(
                    "/swagger",
                    StringComparison.OrdinalIgnoreCase
                )
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
