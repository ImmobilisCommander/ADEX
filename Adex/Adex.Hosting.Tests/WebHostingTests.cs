using Adex.Mvc.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApiHosting = Adex.WebApi.Hosting;

namespace Adex.Hosting.Tests;

public class WebHostingTests
{
    [Fact]
    public async Task MvcPipelineMapsHealthAndApplicationRoutes()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ApplicationName = typeof(Adex.Mvc.Program).Assembly.FullName,
        });
        builder.AddAdexObservability();
        builder.Services.AddAdexMvc().AddAdexApiClient();
        await using var app = builder.Build();

        app.UseAdexPipeline();
        app.MapAdexEndpoints();

        var routes = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText).ToArray();
        Assert.Contains("/health/live", routes);
        Assert.Contains("/health/ready", routes);
        Assert.Contains(routes, route => route != null && route.Contains("Home"));
    }

    [Fact]
    public async Task ApiPipelineMapsHealthAndControllers()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ApplicationName = typeof(Adex.WebApi.Program).Assembly.FullName,
        });
        ApiHosting.ObservabilityExtensions.AddAdexObservability(builder);
        ApiHosting.ApplicationServiceCollectionExtensions.AddAdexWebApi(builder.Services);
        ApiHosting.DatabaseServiceCollectionExtensions.AddAdexDatabase(builder.Services);
        ApiHosting.ApplicationServiceCollectionExtensions.AddAdexServices(builder.Services);
        await using var app = builder.Build();

        ApiHosting.WebApplicationExtensions.UseAdexPipeline(app);
        ApiHosting.WebApplicationExtensions.MapAdexEndpoints(app);

        var routes = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Select(endpoint => endpoint.RoutePattern.RawText).ToArray();
        Assert.Contains("/health/live", routes);
        Assert.Contains("/health/ready", routes);
        Assert.Contains("api/dashboard", routes, StringComparer.OrdinalIgnoreCase);
    }
}
