using RulesEngine.Models;
using System.Text.RegularExpressions;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class DataValidator : IDataValidator
{
    public async Task<bool> ValidateAsync(dynamic data, IEnumerable<Workflow> workflows, IList<string> errors)
    {
        ReSettings reSettings = new() { CustomTypes = [typeof(RegexMatcher)] };
        RulesEngine.RulesEngine rulesEngine = new(workflows.ToArray(), reSettings);
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

/// <summary>
/// A custom matcher class for regex validation in RulesEngine.
/// </summary>
/// <remarks>
/// Can't get Regex.IsMatch to work in RulesEngine, so we create a wrapper class to expose it as a method.
/// </remarks>
public class RegexMatcher
{
    public static bool Match(string? input, string pattern)
    {
        if (input is null) return false;
        return Regex.IsMatch(input, pattern);
    }
}
