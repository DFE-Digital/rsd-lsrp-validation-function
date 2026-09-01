namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface IFileValidationResultService
{
    Task SendResultAsync(string fileId, bool isValid, IEnumerable<string> errors);
}
