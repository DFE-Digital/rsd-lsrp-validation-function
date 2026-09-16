using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using NSubstitute;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class LocalAuthorityProviderTest
{
    private static readonly string? apiKey;

    static LocalAuthorityProviderTest()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<LocalAuthorityProviderTest>()
            .AddEnvironmentVariables()
            .Build();
        apiKey = configuration["LocalAuthorityProvider:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Set 'LocalAuthorityProvider:ApiKey' in user secrets or environment variables.");
    }

    [Category("Integration")]
    [Theory]
    [InlineData("301", "Barking and Dagenham")]
    [InlineData("839", "Bournemouth, Christchurch and Poole")]
    [InlineData("816", "York")]
    public void GetLocalAuthority_ShouldReturnCorrectLocalAuthority(string localAuthorityCode, string expectedName)
    {
        // Arrange
        IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
        HttpClient httpClient = new()
        {
            BaseAddress = new Uri("https://api.dev.academies.education.gov.uk/v4/local-authorities")
        };
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);
        httpClientFactory.CreateClient("LocalAuthorities").Returns(httpClient);
        IDistributedCache distributedCache = Substitute.For<IDistributedCache>();
        var provider = new LocalAuthorityProvider(httpClientFactory, distributedCache);

        // Act
        var result = provider.GetLocalAuthority(localAuthorityCode);

        // Assert
        Assert.Equal(expectedName, result?.Name);
    }
}

public class LocalAuthorityProvider(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
{
    private const string LocalAuthoritiesCacheKey = "LocalAuthorities";
    private readonly JsonSerializerOptions? jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LocalAuthority? GetLocalAuthority(string localAuthorityCode)
    {
        string? cachedJson = distributedCache.GetString(LocalAuthoritiesCacheKey);
        IEnumerable<LocalAuthority>? localAuthorities = null;
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            Debug.WriteLine("Using cached local authorities.");
        }
        else
        {
            Debug.WriteLine("Fetching local authorities from API...");
            HttpClient httpClient = httpClientFactory.CreateClient(LocalAuthoritiesCacheKey);
            cachedJson = httpClient.GetStringAsync("").Result;
            distributedCache.SetString(LocalAuthoritiesCacheKey, cachedJson);
        }
        localAuthorities = JsonSerializer.Deserialize<IEnumerable<LocalAuthority>>(cachedJson, jsonOptions);

        return localAuthorities!.SingleOrDefault(la => la.Code == localAuthorityCode);
    }
}

public sealed class LocalAuthorityExtended
{
    public string? Title { get; set; }
    public string? Region { get; set; }
    public string? URN { get; set; }
    public string? TypeOfCouncil { get; set; }
    public string? PoliticalControl { get; set; }
    public string? DAUCode { get; set; }
}

public static class LocalAuthorityCsvReader
{
    public static List<LocalAuthorityExtended> Read(string csvPath)
    {
        List<LocalAuthorityExtended> result = [];

        using TextFieldParser parser = new(csvPath);
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        _ = parser.ReadFields(); // header row

        while (!parser.EndOfData)
        {
            string[]? row = parser.ReadFields();
            if (row is null || row.Length == 0) continue;

            result.Add(new LocalAuthorityExtended
            {
                Title = Get(row, 0),
                Region = Get(row, 1),
                URN = Get(row, 2),
                TypeOfCouncil = Get(row, 3),
                PoliticalControl = Get(row, 4),
                DAUCode = Get(row, 5)
            });
        }

        return result;
    }

    private static string? Get(string[] row, int index) => index < row.Length && !string.IsNullOrWhiteSpace(row[index]) ? row[index] : null;
}