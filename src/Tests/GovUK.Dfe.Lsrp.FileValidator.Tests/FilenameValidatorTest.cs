using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FilenameValidatorTest(ITestOutputHelper output)
{
    [Theory]
    [InlineData("lsrp-quarterly-return-September-2026-301-barking-and-dagenham.xlsx", true)]
    [InlineData("lsrp-quarterly-return-September-2026-839-bournemouth-christchurch-and-poole.xlsx", true)]
    public async Task ValidateFilename_ShouldReturnExpectedResultAsync(string filename, bool expectedResult)
    {
        // Act
        IConfiguration configuration = CreateConfiguration();
        var localAuthorityProvider = Substitute.For<ILocalAuthorityProvider>();
        localAuthorityProvider.GetLocalAuthorityAsync("301").Returns(new LocalAuthority { Code = "301", Name = "Barking and Dagenham" });
        localAuthorityProvider.GetLocalAuthorityAsync("839").Returns(new LocalAuthority { Code = "839", Name = "Bournemouth, Christchurch and Poole" });
        var validator = new FilenameValidator(configuration, localAuthorityProvider, new TestLogger<FilenameValidator>(output));
        bool result = await validator.ValidateFilenameAsync(filename);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidFilenameAsync()
    {
        // Arrange
        var filename = "invalid-filename.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var localAuthorityProvider = Substitute.For<ILocalAuthorityProvider>();
        var validator = new FilenameValidator(configuration, localAuthorityProvider, new TestLogger<FilenameValidator>(output));
        var result = await validator.ValidateFilenameAsync(filename);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidLocalAuthorityAsync()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-September-2026-999-invalid-local-authority.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var localAuthorityProvider = Substitute.For<ILocalAuthorityProvider>();
        var validator = new FilenameValidator(configuration, localAuthorityProvider, new TestLogger<FilenameValidator>(output));
        var result = await validator.ValidateFilenameAsync(filename);

        // Assert
        Assert.False(result);
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SpreadsheetVersion"] = "September-2026"
            })
            .Build();
}

// TODO move this to main app
public class FilenameValidator(IConfiguration configuration, ILocalAuthorityProvider localAuthorityProvider, ILogger<FilenameValidator> logger)
{
    public async Task<bool> ValidateFilenameAsync(string filename)
    {
        string pattern = @"^lsrp-quarterly-return-[A-Za-z]+-\d{4}-\d{3}-[a-z-]+\.xlsx$";
        bool result = Regex.IsMatch(filename, pattern);
        if (!result)
        {
            logger.LogWarning("Filename does not match the expected pattern.");
            return false;
        }

        FilenameComponents components = GetFilenameComponents(filename);
        var expectedVersion = configuration["SpreadsheetVersion"];
        var actualVersion = $"{components.Month}-{components.Year}";
        if (expectedVersion != actualVersion)
        {
            logger.LogWarning("Spreadsheet version does not match the expected version. Expected: {expectedVersion}, Actual: {actualVersion}", expectedVersion, actualVersion);
            return false;
        }

        var localAuthority = await localAuthorityProvider.GetLocalAuthorityAsync(components.LaCode);
        if (localAuthority == null)
        {
            logger.LogWarning("Local authority code {laCode} ({laName}) is not valid.", components.LaCode, components.LaName);
            return false;
        }

        // TODO check LA code against that from message body (if available) and log a warning if they don't match

        return true;
    }

    private FilenameComponents GetFilenameComponents(string filename)
    {
        string[] parts = filename.Split('-');
        string month = parts[3];
        string year = parts[4];
        string laCode = parts[5];

        int index = filename.IndexOf(laCode) + laCode.Length + 1;
        string laName = filename[index..].Replace(".xlsx", "");

        logger.LogInformation("Extracted month: {month}, year: {year}, local authority code: {laCode}, local authority name: {laName} from filename.", month, year, laCode, laName);
        return new FilenameComponents(month, year, laCode, laName);
    }

    private record FilenameComponents(string Month, string Year, string LaCode, string LaName);
}