using RulesEngine.Models;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class DataValidator : IDataValidator
{
    public async Task<bool> ValidateAsync(dynamic data, IEnumerable<Workflow> workflows, IList<string> errors)
    {
        RulesEngine.RulesEngine rulesEngine = new(workflows.ToArray());
        foreach (var workflow in workflows)
        {
            await RunWorkflowAsync(data, rulesEngine, errors, workflow);
        }

        return !errors.Any();
    }

    private static async Task RunWorkflowAsync(dynamic data, RulesEngine.RulesEngine rulesEngine, IList<string> errors, Workflow workflow)
    {
        IEnumerable<RuleResultTree> results = await rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, data);
        foreach (RuleResultTree? result in results.Where(x => !x.IsSuccess))
        {
            errors.Add($"{workflow.WorkflowName} {result.Rule}: {result.ExceptionMessage}");
        }
    }
}
