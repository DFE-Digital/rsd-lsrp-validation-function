using Azure.Monitor.OpenTelemetry.Exporter;
using GovUK.Dfe.Lsrp.FileValidator;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Services.Configure<ValidationOptions>(builder.Configuration.GetSection("ValidationOptions"));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddHttpClient();
builder.Services.AddScoped<IMessageParser, MessageParser>();
builder.Services.AddScoped<ISpreadsheetValidationService, SpreadsheetValidationService>();
builder.Services.AddScoped<IDataValidator, DataValidator>();
builder.Services.AddScoped<IFileProvider, FileProvider>();
builder.Services.AddScoped<ISpreadsheetDataProvider, SpreadsheetDataProvider>();

builder.Build().Run();
