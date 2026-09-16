namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FilenameValidatorTest
{
    [Fact]
    public void ValidateFilename_ShouldReturnTrueForValidFilename()
    {
        // Arrange
        var filename = "lsrp-quarterly-return-September-2026-839-bournemouth-christchurch-and-poole.xlsx";

        // Act
        var result = FilenameValidator.ValidateFilename(filename);

        // Assert
        Assert.True(result);
    }
}

public static class FilenameValidator
{
    public static bool ValidateFilename(string filename)
    {
        // Simple validation logic for demonstration purposes
        return filename.StartsWith("lsrp-quarterly-return-") && filename.EndsWith(".xlsx");
    }
}