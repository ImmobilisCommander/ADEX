using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Serilog;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Hosting
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseAdexPipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging();
            app.UseExceptionHandler("/Home/Error");
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
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
