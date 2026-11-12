using GovUK.Dfe.Lsrp.FileValidator.Models;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using System.Text.RegularExpressions;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class DataValidator(ILogger<DataValidator> logger) : IDataValidator
{
    public async Task<bool> ValidateAsync(dynamic data, LocalAuthority localAuthority, IEnumerable<Workflow> workflows, IList<string> errors)
    {
        ReSettings reSettings = new() { CustomTypes = [typeof(Regex)] };
        RulesEngine.RulesEngine rulesEngine = new(workflows.ToArray(), reSettings);
        foreach (var workflow in workflows)
        {
            await RunWorkflowAsync(data, localAuthority, rulesEngine, errors, workflow);
        }

        return !errors.Any();
    }

    private async Task RunWorkflowAsync(dynamic data, LocalAuthority localAuthority, RulesEngine.RulesEngine rulesEngine, IList<string> errors, Workflow workflow)
    {
        RuleParameter[] ruleParams = [new("data", data), new("localAuthority", localAuthority)];
        IEnumerable<RuleResultTree> results = await rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, ruleParams);

        foreach (RuleResultTree? result in results.Where(x => !x.IsSuccess))
        {
            logger.LogWarning("Validation failed for workflow {WorkflowName}, rule {RuleName}. Exception: {ExceptionMessage}", workflow.WorkflowName, result.Rule, result.ExceptionMessage);
            errors.Add(result.ExceptionMessage);
        }
    }
}
