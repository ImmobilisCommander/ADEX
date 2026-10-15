using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Adex.Importer.Hosting
{
    public static class HostExtensions
    {
        /// <summary>Exécute l'import et retourne le code de sortie du processus.</summary>
        public static async Task<int> RunAdexImporterAsync(this IHost host, string[] args)
        {
            try
            {
                using var shutdown = new CancellationTokenSource();
                Console.CancelKeyPress += (_, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    shutdown.Cancel();
                };

                var assumeYes = args.Contains("--yes", StringComparer.OrdinalIgnoreCase);
                await using var scope = host.Services.CreateAsyncScope();
                return await scope
                    .ServiceProvider.GetRequiredService<ImportRunner>()
                    .RunAsync(assumeYes, shutdown.Token);
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
