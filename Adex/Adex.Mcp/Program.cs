using System.Threading.Tasks;
using Adex.Mcp.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Adex.Mcp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddAdexObservability();
            builder.Services.AddAdexMcp();

            await using var app = builder.Build();
            app.MapAdexEndpoints();
            await app.RunAsync();
        }
    }
}
