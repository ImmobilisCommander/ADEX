using System;
using System.Threading.Tasks;
using Adex.Importer.Hosting;
using Microsoft.Extensions.Hosting;

namespace Adex.Importer
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(
                new HostApplicationBuilderSettings
                {
                    Args = args,
                    ContentRootPath = AppContext.BaseDirectory,
                }
            );
            builder.ConfigureAdexImporterHost();
            builder.Services.AddAdexImporter();

            using var host = builder.Build();
            return await host.RunAdexImporterAsync(args);
        }
    }
}
