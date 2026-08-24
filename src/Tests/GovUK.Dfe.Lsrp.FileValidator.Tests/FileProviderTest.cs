using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests
{
    public class FileProviderTest(ITestOutputHelper testOutput)
    {
        [Fact(Skip = "Requires valid Azure Storage file URL with SAS key")]
        public async Task GetFileAsync_ReturnsStream()
        {
            // Arrange
            HttpClient httpClient = new();
            IHttpClientFactory httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(httpClient);
            string fileUri = GetFileUriFromUserSecrets();

            // Act
            FileProvider fileProvider = new(httpClientFactory);
            Stream result = await fileProvider.GetFileAsync(fileUri);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MemoryStream>(result);
            Assert.True(result.Length > 0);
            testOutput.WriteLine($"Retrieved file with length {result.Length} bytes.");
        }

        /// <summary>
        /// Get file storage URL including SAS key from user secrets.
        /// </summary>
        private static string GetFileUriFromUserSecrets()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddUserSecrets("aed34d92-ec3c-4748-b49b-a06c668bfad7").Build();
            string? fileUri = configuration["FileProvider:FileUri"] ?? configuration["FileUri"];
            return !string.IsNullOrWhiteSpace(fileUri)
                ? fileUri
                : throw new InvalidOperationException("Set user secret 'FileProvider:FileUri' (or 'FileUri') with an Azure Storage file URL.");
        }
    }
}
