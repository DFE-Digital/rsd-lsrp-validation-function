namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetValidationService
{
    Task<bool> ValidateAsync(string filename, List<string> errors);
}
