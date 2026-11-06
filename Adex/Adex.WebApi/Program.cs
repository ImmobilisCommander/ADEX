using System.Threading.Tasks;
using Adex.WebApi.Hosting;
using Microsoft.AspNetCore.Builder;

namespace Adex.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureAdexHost();
            builder.AddAdexObservability();
            builder.Services.AddAdexWebApi().AddAdexDatabase().AddAdexServices();

            await using var app = builder.Build();
            app.UseAdexPipeline();
            app.MapAdexEndpoints();
            await app.RunAdexAsync();
        }
    }
}
