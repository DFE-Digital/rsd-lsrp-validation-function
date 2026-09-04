using Azure.Monitor.OpenTelemetry.Exporter;
using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var validationSection = builder.Configuration.GetSection("ValidationOptions");
builder.Services.Configure<ValidationOptions>(validationSection);

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry().UseFunctionsWorkerDefaults().UseAzureMonitorExporter();
}

builder.Services.AddHttpClient();
builder.Services.AddScoped<ISpreadsheetValidationService, SpreadsheetValidationService>();
builder.Services.AddScoped<IDataValidator, DataValidator>();
builder.Services.AddScoped<IFileProvider, FileProvider>();
builder.Services.AddScoped<ISpreadsheetDataProvider, SpreadsheetDataProvider>();
builder.Services.AddScoped<IFileValidationResultService, FileValidationResultService>();

builder.Build().Run();
