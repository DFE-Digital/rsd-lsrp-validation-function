using RulesEngine.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public interface IDataValidator
{
    Task<bool> ValidateAsync(dynamic data, IEnumerable<Workflow> workflows, IList<string> errors);
}
