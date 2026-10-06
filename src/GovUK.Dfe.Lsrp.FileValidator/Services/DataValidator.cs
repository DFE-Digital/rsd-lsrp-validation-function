using GovUK.Dfe.Lsrp.FileValidator.Models;
using RulesEngine.Models;
using System.Text.RegularExpressions;

namespace GovUK.Dfe.Lsrp.FileValidator.Services;

public class DataValidator : IDataValidator
{
    public async Task<bool> ValidateAsync(dynamic data, LocalAuthority localAuthority, IEnumerable<Workflow> workflows, IList<string> errors)
    {
        ReSettings reSettings = new() { CustomTypes = [typeof(Utils)] };
        RulesEngine.RulesEngine rulesEngine = new(workflows.ToArray(), reSettings);
        foreach (var workflow in workflows)
        {
            await RunWorkflowAsync(data, localAuthority, rulesEngine, errors, workflow);
        }

        return !errors.Any();
    }

    private static async Task RunWorkflowAsync(dynamic data, LocalAuthority localAuthority, RulesEngine.RulesEngine rulesEngine, IList<string> errors, Workflow workflow)
    {
        RuleParameter[] ruleParams = [new("data", data), new("localAuthority", localAuthority)];
        IEnumerable<RuleResultTree> results = await rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, ruleParams);

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
    /// <summary>
    /// Checks if the given year string is in the format "20xx-yy Quarterly Data" and represents consecutive years.
    /// </summary>
    public static bool CheckYear(string year)
    {
        if (string.IsNullOrEmpty(year))
        {
            return false;
        }

        var isValid = Regex.IsMatch(year, "^20\\d{2}-\\d{2} Quarterly Data$");
        if (!isValid)
        {
            return false;
        }

        string[] years = year.Split('-');
        int year1 = int.Parse(years[0]);
        int year2 = int.Parse(years[1][..2]) + 2000; // Convert yy to yyyy
        return year2 - year1 == 1;
    }

    /// <summary>
    /// Checks if given local authorities match (code only)
    /// </summary>
    /// <remarks>
    /// Only the local authority code is checked, not the name, as the name may vary in formatting.
    /// </remarks>
    public static bool CheckLocalAuthority(string? actualLocalAuthority, LocalAuthority expectedLocalAuthority)
    {
        if (string.IsNullOrEmpty(actualLocalAuthority))
        {
            return false;
        }

        var localAuthorityCode = actualLocalAuthority.Split(' ')[0]; // Get the first part of the string before any space
        return string.Equals(localAuthorityCode, expectedLocalAuthority.Code, StringComparison.OrdinalIgnoreCase);
    }
}
