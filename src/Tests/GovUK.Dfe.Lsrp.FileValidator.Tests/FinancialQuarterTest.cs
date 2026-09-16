using GovUK.Dfe.Lsrp.FileValidator.Models;
using Xunit.Abstractions;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class FinancialQuarterTest(ITestOutputHelper output)
{
    [Theory]
    [InlineData("Q1", "2026-04-01", "June-2026")]
    [InlineData("Q2", "2026-07-01", "September-2026")]
    [InlineData("Q3", "2026-10-01", "December-2026")]
    [InlineData("Q4", "2027-01-01", "March-2027")]
    public void GetQuarterEnd_ShouldReturnCorrectQuarter(string quarter, DateTime date, string quarterEnd)
    {
        var result = FinancialQuarter.GetQuarterEnd(date);
        Assert.Equal(quarterEnd, result);
        output.WriteLine($"{quarter}: {date:d}, {result}.");
    }
}
