using System.Threading.Tasks;
using Adex.Mvc.Hosting;
using Microsoft.AspNetCore.Builder;

namespace Adex.Mvc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureAdexHost();
            builder.AddAdexObservability();
            builder.Services.AddAdexMvc().AddAdexApiClient();

            await using var app = builder.Build();
            app.UseAdexPipeline();
            app.MapAdexEndpoints();
            await app.RunAdexAsync();
        }
    }
}
