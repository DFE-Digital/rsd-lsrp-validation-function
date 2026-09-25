using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FilenameValidator(IConfiguration configuration, ILogger<FilenameValidator> logger) : IFilenameValidator
{
    public async Task<bool> ValidateFilenameAsync(string filename, LocalAuthority localAuthority, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));

        if (!filename.StartsWith("lsrp-quarterly-return-", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Filename does not start with the expected prefix 'lsrp-quarterly-return-'.");
            AddError(errors, "FilePrefixIncorrectMessage");
            return false;
        }

        if (!filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Filename does not have the expected .xlsx extension.");
            AddError(errors, "FileExtensionIncorrectMessage");
            return false;
        }

        FilenameComponents? components = GetFilenameComponents(filename);
        if (components == null)
        {
            logger.LogWarning("Filename does not have the expected format.");
            AddError(errors, "FilenameFormatIncorrectMessage");
            return false;
        }

        var expectedVersion = configuration["SpreadsheetVersion"];
        var actualVersion = $"{components.Month}-{components.Year}";
        if (!string.Equals(expectedVersion, actualVersion, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Spreadsheet version does not match the expected version. Expected: {expectedVersion}, Actual: {actualVersion}", expectedVersion, actualVersion);
            AddError(errors, "SpreadsheetVersionIncorrectMessage");
            return false;
        }

        if (localAuthority.Code != components.LaCode)
        {
            logger.LogWarning("Local authority code {laCode} ({laName}) does not match that in message {messageLaCode}.", components.LaCode, components.LaName, localAuthority.Code);
            AddError(errors, "LocalAuthorityMismatchMessage");
            return false;
        }

        return true;
    }

    private void AddError(List<string> errors, string key) => errors.Add(GetMessage(key));

    private string GetMessage(string key) => configuration[key] ?? $"{key} missing in configuration";

    private FilenameComponents? GetFilenameComponents(string filename)
    {
        string[] parts = filename.Split('-');
        if (parts.Length < 6) return null;

        string month = parts[3];
        string year = parts[4];
        string laCode = parts[5];

        int index = filename.LastIndexOf($"{laCode}-", StringComparison.Ordinal) + laCode.Length + 1;
        string laName = filename[index..].Replace(".xlsx", "");

        logger.LogInformation("Extracted month: {month}, year: {year}, local authority code: {laCode}, local authority name: {laName} from filename.", month, year, laCode, laName);
        return new FilenameComponents(month, year, laCode, laName);
    }

    private record FilenameComponents(string Month, string Year, string LaCode, string LaName);
}