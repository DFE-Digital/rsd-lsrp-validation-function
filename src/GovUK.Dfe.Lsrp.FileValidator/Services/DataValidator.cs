using RulesEngine.Models;
using System.Text.RegularExpressions;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class DataValidator : IDataValidator
{
    public async Task<bool> ValidateAsync(dynamic data, IEnumerable<Workflow> workflows, IList<string> errors)
    {
        ReSettings reSettings = new() { CustomTypes = [typeof(Utils)] };
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
/// A custom utility class for use in RulesEngine.
/// </summary>
public class Utils
{
    /// <remarks>
    /// Can't get Regex.IsMatch to work in RulesEngine, so we create a wrapper class to expose it as a method.
    /// </remarks>
    public static bool MatchRegex(string? input, string pattern)
    {
        if (input is null) return false;
        return Regex.IsMatch(input, pattern);
    }
}
