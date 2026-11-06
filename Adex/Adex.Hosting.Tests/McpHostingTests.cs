using Adex.Mcp.Hosting;
using Adex.WebApi.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Adex.Hosting.Tests;

public class McpHostingTests
{
    [Fact]
    public async Task MapsMcpAndHealthEndpointsAndServesLiveness()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ApplicationName = typeof(Adex.Mcp.Program).Assembly.FullName,
        });
        builder.Configuration["AdexApi:BaseAddress"] = "http://localhost:5002/";
        builder.Configuration["AllowedHosts"] = "*";
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.AddAdexObservability();
        builder.Services.AddAdexMcp();
        await using var app = builder.Build();
        app.MapAdexEndpoints();

        await app.StartAsync(TestContext.Current.CancellationToken);
        var routes = app.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText).ToArray();
        Assert.Contains("/mcp/", routes);
        Assert.Contains("/health/live", routes);
        Assert.Contains("/health/ready", routes);
        Assert.NotNull(app.Services.GetRequiredService<IAdexWebApiClient>());

        var address = Assert.Single(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses);
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { BaseAddress = new Uri(address) };
        using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
