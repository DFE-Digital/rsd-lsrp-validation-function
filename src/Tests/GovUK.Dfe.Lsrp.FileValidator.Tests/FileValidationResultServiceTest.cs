using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FileValidationResultServiceTest(ITestOutputHelper output)
{
    [Fact(Skip = "Requires real file ID")]
    [Trait("Category", "Integration")]
    public async Task SendResultAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(httpClient);
        IConfiguration configuration = TestConfig.GetConfiguration();
        var logger = new TestLogger<FileValidationResultService>(output);
        var service = new FileValidationResultService(httpClientFactory, configuration, logger);
        var fileId = "test-application-id";
        var errors = new List<string> { "Test error message", "Another test error message" };

        // Act
        await service.SendResultAsync(fileId, false, errors);

        // Assert
        // No exception means the test passes
    }

    [Fact]
    public async Task SendResultAsync_ShouldThrowArgumentException_WhenFileIdIsEmpty()
    {
        // Arrange
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        IConfiguration configuration = TestConfig.GetConfiguration();
        var logger = new TestLogger<FileValidationResultService>(output);
        var service = new FileValidationResultService(httpClientFactory, configuration, logger);
        var errors = new List<string> { "Test error message" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendResultAsync(string.Empty, false, errors));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SendResultAsync_ShouldHandleErrorResponse()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(httpClient);
        IConfiguration configuration = TestConfig.GetConfiguration();
        var logger = new TestLogger<FileValidationResultService>(output);
        var service = new FileValidationResultService(httpClientFactory, configuration, logger);
        var errors = new List<string> { "Test error message", "Another test error message" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.SendResultAsync("test", false, errors));
        output.WriteLine($"Caught expected HttpRequestException: {exception.Message}");
    }
}
