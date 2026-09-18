using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FilenameValidator(IConfiguration configuration, ILogger<FilenameValidator> logger) : IFilenameValidator
{
    public async Task<bool> ValidateFilenameAsync(string filename, LocalAuthority localAuthority, List<string> errors)
    {
        string pattern = @"^lsrp-quarterly-return-[A-Za-z]+-\d{4}-\d{3}-[a-z-]+\.xlsx$";
        bool result = Regex.IsMatch(filename, pattern);
        if (!result)
        {
            logger.LogWarning("Filename does not match the expected pattern.");
            errors.Add("Filename does not match the expected pattern.");
            return false;
        }

        FilenameComponents components = GetFilenameComponents(filename);
        var expectedVersion = configuration["SpreadsheetVersion"];
        var actualVersion = $"{components.Month}-{components.Year}";
        if (!string.Equals(expectedVersion, actualVersion, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Spreadsheet version does not match the expected version. Expected: {expectedVersion}, Actual: {actualVersion}", expectedVersion, actualVersion);
            errors.Add($"Spreadsheet version does not match the expected version. Expected: {expectedVersion}, Actual: {actualVersion}");
            return false;
        }

        if (localAuthority.Code != components.LaCode)
        {
            logger.LogWarning("Local authority code {laCode} ({laName}) does not match that in message {messageLaCode}.", components.LaCode, components.LaName, localAuthority.Code);
            errors.Add($"Local authority code {components.LaCode} ({components.LaName}) does not match that in message {localAuthority.Code} ({localAuthority.Name}).");
            return false;
        }

        return true;
    }

    private FilenameComponents GetFilenameComponents(string filename)
    {
        string[] parts = filename.Split('-');
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