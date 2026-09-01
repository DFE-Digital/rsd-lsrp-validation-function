using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FileValidationResultService : IFileValidationResultService
{
    private readonly string TenantId;
    private readonly string FilesUrl;
    private readonly string ApiKey;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<FileValidationResultService> logger;

    public FileValidationResultService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FileValidationResultService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;

        TenantId = configuration["TenantId"]!;
        if (string.IsNullOrWhiteSpace(TenantId))
        {
            throw new ArgumentException("TenantId configuration is empty or whitespace");
        }

        FilesUrl = configuration["ValidationResultFilesUrl"]!;
        if (string.IsNullOrWhiteSpace(FilesUrl))
        {
            throw new ArgumentException("ValidationResultFilesUrl configuration is empty or whitespace");
        }

        ApiKey = configuration["ValidationResultApiKey"]!;
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentException("ValidationResultApiKey configuration is empty or whitespace");
        }
    }

    public async Task SendResultAsync(string fileId, bool isValid, IEnumerable<string> errors)
    {
        HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("X-Tenant-ID", TenantId);
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);

        FileValidationResultDto dto = new()
        {
            IsValid = isValid,
            Message = string.Join(", ", errors),
            Source = fileId
        };

        string? url = $"{FilesUrl}/{fileId}/validation-result";
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(url, dto);
        string responseContent = await response.Content.ReadAsStringAsync();
        logger.LogError("Response Content: {responseContent}", responseContent);
        response.EnsureSuccessStatusCode();
    }
}
