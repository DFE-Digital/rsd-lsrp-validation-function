using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetValidationService
{
    Task<bool> ValidateAsync(User user, string fileUri, List<string> errors);
}
