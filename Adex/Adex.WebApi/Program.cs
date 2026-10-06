using System;
using Adex.Business;
using Adex.Common;
using Adex.Data.MetaModel;
using Adex.Data.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

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

var adexConnectionString = GetRequiredConnectionString("Adex");
var metadataConnectionString = GetRequiredConnectionString("AdexMeta");

builder.Services.AddControllers();
builder.Services.AddDbContextFactory<AdexContext>(options =>
    options.UseNpgsql(adexConnectionString)
);
builder.Services.AddDbContextFactory<AdexMetaContext>(options =>
    options.UseNpgsql(metadataConnectionString)
);
builder.Services.AddScoped(provider =>
{
    var loader = new CsvLoaderNormalized(
        provider.GetRequiredService<IDbContextFactory<AdexContext>>()
    );
    AttachLogging(provider.GetRequiredService<ILogger<CsvLoaderNormalized>>(), loader);
    return loader;
});
builder.Services.AddScoped<ILinkSearchService>(provider =>
    provider.GetRequiredService<CsvLoaderNormalized>()
);
builder.Services.AddScoped(provider =>
{
    var loader = new CvsLoaderMetadata(metadataConnectionString);
    AttachLogging(provider.GetRequiredService<ILogger<CvsLoaderMetadata>>(), loader);
    return loader;
});
builder.Services.AddScoped<IMetadataLookupService>(provider =>
    provider.GetRequiredService<CvsLoaderMetadata>()
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "ADEX API", Version = "v1" })
);

var app = builder.Build();
NpgsqlLoggingConfiguration.InitializeLogging(
    app.Services.GetRequiredService<ILoggerFactory>(),
    parameterLoggingEnabled: false
);
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

string GetRequiredConnectionString(string name)
{
    return builder.Configuration.GetConnectionString(name)
        ?? throw new InvalidOperationException(
            $"The '{name}' connection string is not configured."
        );
}

static void AttachLogging<T>(ILogger<T> logger, ICsvLoader loader)
{
    loader.OnMessage += (_, message) =>
    {
        switch (message.Level)
        {
            case Level.Debug:
                logger.LogDebug("{LoaderMessage}", message.Message);
                break;
            case Level.Info:
                logger.LogInformation("{LoaderMessage}", message.Message);
                break;
            case Level.Warn:
                logger.LogWarning("{LoaderMessage}", message.Message);
                break;
            case Level.Error:
                logger.LogError("{LoaderMessage}", message.Message);
                break;
        }
    };
}
