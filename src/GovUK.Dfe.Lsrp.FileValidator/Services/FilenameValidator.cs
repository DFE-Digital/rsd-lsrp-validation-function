using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class FilenameValidator(IConfiguration configuration, ILogger<FilenameValidator> logger) : IFilenameValidator
{
    public async Task<bool> ValidateFilenameAsync(string filename, LocalAuthority localAuthority, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));

        var expectedPrefix = configuration["FilenamePrefix"];
        if (string.IsNullOrWhiteSpace(expectedPrefix))
        {
            throw new InvalidOperationException("Expected filename prefix is not configured.");
        }

        if (!filename.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Filename does not start with the expected prefix '{expectedPrefix}'.", expectedPrefix);
            AddError(errors, "FilePrefixIncorrectMessage");
            return false;
        }

        if (!filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Filename does not have the expected .xlsx extension.");
            AddError(errors, "FileExtensionIncorrectMessage");
            return false;
        }

        string? laCode = GetLaCode(filename, expectedPrefix);
        if (laCode == null)
        {
            logger.LogWarning("Filename does not have the expected format.");
            AddError(errors, "FilenameFormatIncorrectMessage");
            return false;
        }

        if (!string.Equals(localAuthority.Code, laCode, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Local authority code {laCode} does not match that in message {messageLaCode}.", laCode, localAuthority.Code);
            AddError(errors, "LocalAuthorityMismatchMessage");
            return false;
        }

        return true;
    }

    private void AddError(List<string> errors, string key) => errors.Add(GetMessage(key));

    private string GetMessage(string key) => configuration[key] ?? $"{key} missing in configuration";

    private string? GetLaCode(string filename, string expectedPrefix)
    {
        int index = filename.IndexOf(expectedPrefix, StringComparison.OrdinalIgnoreCase);
        string la = filename[(index + expectedPrefix.Length)..];
        string[] parts = la.Split('-');
        if (parts.Length < 1) return null;

        string laCode = parts[0];

        index = filename.LastIndexOf($"{laCode}-", StringComparison.Ordinal) + laCode.Length + 1;
        string laName = filename[index..].Replace(".xlsx", "");

        logger.LogInformation("Extracted local authority code: {laCode}, local authority name: {laName} from filename.", laCode, laName);
        return laCode;
    }
}