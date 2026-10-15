using Serilog;

namespace Adex.Yarp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog(
                (context, configuration) =>
                    configuration.ReadFrom.Configuration(context.Configuration)
            );

            builder
                .Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            await using var app = builder.Build();
            app.MapReverseProxy();
            await app.RunAsync();
        }
    }
}
