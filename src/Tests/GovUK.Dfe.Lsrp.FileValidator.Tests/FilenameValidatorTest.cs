using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FilenameValidatorTest(ITestOutputHelper output)
{
    [Fact]
    public async Task ValidateFilename_ShouldReturnTrueForValidFilenameAsync()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-september-2026-301-barking-and-dagenham.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        bool result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }
    [Fact]
    public async Task ValidateFilename_ShouldReturnTrueForValidUppercaseFilenameAsync()
    {
        // Arrange
        var filename = "LSRP-QUARTERLY-RETURN-SEPTEMBER-2026-301-BARKING-AND-DAGENHAM.XLSX";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        bool result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidFilenameAsync()
    {
        // Arrange
        var filename = "invalid-filename.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        var result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(errors);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForNonMatchingLocalAuthorityAsync()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-September-2026-999-invalid-local-authority.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        var result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(errors);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidVersionAsync()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-June-2026-301-barking-and-dagenham.xlsx";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        var result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(errors);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidExtensionAsync()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-September-2026-301-barking-and-dagenham.csv";

        // Act
        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        var result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(errors);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
    }

    [Fact]
    public async Task ValidateFilename_ShouldReturnFalseForInvalidComponentsAsync()
    {
        var filename = "lsrp-quarterly-return-.xlsx";

        IConfiguration configuration = CreateConfiguration();
        var validator = new FilenameValidator(configuration, new TestLogger<FilenameValidator>(output));
        List<string> errors = [];
        var result = await validator.ValidateFilenameAsync(filename, new LocalAuthority { Code = "301", Name = "Barking and Dagenham" }, errors);

        Assert.False(result);
        Assert.NotEmpty(errors);
        output.WriteLine($"Errors: {string.Join(", ", errors)}");
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SpreadsheetVersion"] = "September-2026"
            })
            .Build();
}

