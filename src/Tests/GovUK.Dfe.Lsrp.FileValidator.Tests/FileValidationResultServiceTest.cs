using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FileValidationResultServiceTest(ITestOutputHelper output)
{
    [Fact(Skip = "Integration test requiring file ID")]
    public async Task SendResultAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var httpClient = new HttpClient();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(httpClient);
        var configuration = Substitute.For<IConfiguration>();
        configuration["TenantId"].Returns("22222222-2222-4222-8222-222222222222");
        configuration["ValidationResultFilesUrl"].Returns("https://api-dev-flexforms.rsd.education.gov.uk/v1/integrations/files");
        configuration["ValidationResultApiKey"].Returns("local-file-validation-dev-key");
        var logger = new FakeLogger(output);
        var service = new FileValidationResultService(httpClientFactory, configuration, logger);
        var fileId = "test-application-id";
        var errors = new List<string> { "Test error message", "Another test error message" };

        // Act
        await service.SendResultAsync(fileId, false, errors);

        // Assert
        // No exception means the test passes
    }

    class FakeLogger(ITestOutputHelper output) : ILogger<FileValidationResultService>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            output.WriteLine(formatter(state, exception));
        }
    }
}
