
using Datahub.Core;
using Datahub.Metadata.Model;
using Datahub.Portal;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Logging.AddEventLog();
}

// Application Insights logging filters
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Trace);
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Trace);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogLevel.Information);
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogLevel.Information);

// App configuration
builder.Configuration.AddUserSecrets<Startup>();

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services from Startup
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline
var logger = app.Services.GetRequiredService<ILogger<Startup>>();
var configuration = app.Services.GetRequiredService<IConfiguration>();
var metadataFactory = app.Services.GetRequiredService<IDbContextFactory<MetadataDbContext>>();
startup.Configure(app, builder.Environment, logger, configuration, metadataFactory);

app.Run();
