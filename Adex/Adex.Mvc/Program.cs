using System;
using Adex.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<AdexApiClient>(client =>
{
    var baseAddress = builder.Configuration["AdexApi:BaseAddress"];
    if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException(
            "The 'AdexApi:BaseAddress' setting must be an absolute URI."
        );
    }

    client.BaseAddress = uri;
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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
