using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Adex.Mvc.Hosting
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseAdexPipeline(this WebApplication app)
        {
            // Le tunnel Cloudflare arrive en HTTP depuis loopback : restaurer le schéma et l'IP d'origine.
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor
                        | ForwardedHeaders.XForwardedProto
                        | ForwardedHeaders.XForwardedHost,
                }
            );
            app.UseSerilogRequestLogging();
            app.UseMiddleware<MarkdownNegotiationMiddleware>();
            app.UseExceptionHandler("/Home/Error");
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready");
            app.MapControllerRoute(
                name: "entity",
                pattern: "{id:guid}",
                defaults: new { controller = "Entity", action = "Details" }
            );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
            return app;
        }

        public static async Task RunAdexAsync(this WebApplication app)
        {
            try
            {
                using var shutdown = new CancellationTokenSource();
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };
                await app.RunAsync(shutdown.Token);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
