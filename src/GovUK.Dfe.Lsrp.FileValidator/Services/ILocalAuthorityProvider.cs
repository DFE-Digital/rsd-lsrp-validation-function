using GovUK.Dfe.Lsrp.FileValidator.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services
{
    public interface ILocalAuthorityProvider
    {
        Task<LocalAuthority?> GetLocalAuthorityAsync(string localAuthorityCode);
    }
}