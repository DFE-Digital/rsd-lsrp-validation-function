using GovUK.Dfe.Lsrp.FileValidator.Models;
using RulesEngine.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface IDataValidator
{
    Task<bool> ValidateAsync(dynamic data, LocalAuthority localAuthority, IEnumerable<Workflow> workflows, IList<string> errors);
}
