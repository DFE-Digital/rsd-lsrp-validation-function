using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services
{
    public interface IFilenameValidator
    {
        Task<bool> ValidateFilenameAsync(string filename, LocalAuthority localAuthority, List<string> errors);
    }
}