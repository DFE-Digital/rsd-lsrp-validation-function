using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class LocalAuthorityProviderTest(ITestOutputHelper output)
{
    private static readonly string? apiKey;
    private static readonly MemoryDistributedCache distributedCache;
    private static readonly HttpClient httpClient;

    static LocalAuthorityProviderTest()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<LocalAuthorityProviderTest>()
            .AddEnvironmentVariables()
            .Build();
        apiKey = configuration["LocalAuthorityProvider:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("Set 'LocalAuthorityProvider:ApiKey' in user secrets or environment variables.");

        httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.dev.academies.education.gov.uk/v4/")
        };
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

        distributedCache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
    }

    [Trait("Category", "Integration")]
    [Theory]
    [InlineData("301", "Barking and Dagenham")]
    [InlineData("839", "Bournemouth, Christchurch and Poole")]
    [InlineData("816", "York")]
    public async Task GetLocalAuthority_ShouldReturnCorrectLocalAuthorityAsync(string localAuthorityCode, string localAuthorityName)
    {
        // Arrange
        IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(LocalAuthorityProvider.LocalAuthoritiesKey).Returns(httpClient);
        TestLogger<LocalAuthorityProvider> logger = new(output);
        LocalAuthorityProvider provider = new(httpClientFactory, distributedCache, logger);

        // Act
        LocalAuthority? result = await provider.GetLocalAuthorityAsync(localAuthorityCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(localAuthorityCode, result.Code);
        Assert.Equal(localAuthorityName, result.Name);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetLocalAuthority_ShouldReturnNullForInvalidCodeAsync()
    {
        // Arrange
        IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(LocalAuthorityProvider.LocalAuthoritiesKey).Returns(httpClient);
        TestLogger<LocalAuthorityProvider> logger = new(output);
        LocalAuthorityProvider provider = new(httpClientFactory, distributedCache, logger);

        // Act
        LocalAuthority? result = await provider.GetLocalAuthorityAsync("9999");

        // Assert
        Assert.Null(result);
    }
}
