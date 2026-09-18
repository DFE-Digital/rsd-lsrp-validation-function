namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface ISpreadsheetValidationService
{
    Task<bool> ValidateAsync(MessageData messageData, List<string> errors);
}
