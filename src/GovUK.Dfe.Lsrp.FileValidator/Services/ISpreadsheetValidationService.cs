namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetValidationService
{
    Task<bool> ValidateAsync(string localAuthority, string fileUri, List<string> errors);
}
