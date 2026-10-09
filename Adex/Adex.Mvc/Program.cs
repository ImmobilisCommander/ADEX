using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Adex.Mvc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseDefaultServiceProvider(
                (_, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                }
            );
            builder.Host.UseSerilog(
                (context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
            );

            builder.Services.AddControllersWithViews();
            builder.Services.AddOptions<AdexApiOptions>()
                .BindConfiguration(AdexApiOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            builder.Services.AddHttpClient<AdexApiClient>((serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(
                    serviceProvider.GetRequiredService<IOptions<AdexApiOptions>>().Value.BaseAddress
                );
                // Scanning the whole CSV can be long; the call is cancelled when the user leaves the page.
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

            var app = builder.Build();
            app.UseSerilogRequestLogging();
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
