using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class LocalAuthorityProvider(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache, ILogger<LocalAuthorityProvider> logger)
{
    public const string LocalAuthoritiesKey = "LocalAuthorities";
    private readonly JsonSerializerOptions? jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<LocalAuthority?> GetLocalAuthorityAsync(string localAuthorityCode)
    {
        string? cachedJson = await distributedCache.GetStringAsync(LocalAuthoritiesKey);
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            logger.LogInformation("Using cached local authorities.");
        }
        else
        {
            logger.LogInformation("Fetching local authorities from API...");
            HttpClient httpClient = httpClientFactory.CreateClient(LocalAuthoritiesKey);
            cachedJson = await httpClient.GetStringAsync("local-authorities");
            await distributedCache.SetStringAsync(LocalAuthoritiesKey, cachedJson);
        }
        IEnumerable<LocalAuthority>? localAuthorities = JsonSerializer.Deserialize<IEnumerable<LocalAuthority>>(cachedJson, jsonOptions);

        return localAuthorities!.SingleOrDefault(la => la.Code == localAuthorityCode);
    }
}
