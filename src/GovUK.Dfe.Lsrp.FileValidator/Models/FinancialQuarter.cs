namespace GovUK.Dfe.Lsrp.FileValidator.Models;

public class FinancialQuarter
{
    private static readonly IEnumerable<QuarterEndMonth> QuarterEndMonths =
    [
        new QuarterEndMonth(1, 3, "March"),
        new QuarterEndMonth(4, 6, "June"),
        new QuarterEndMonth(7, 9, "September"),
        new QuarterEndMonth(10, 12, "December"),
    ];


    public static string GetQuarterEnd(DateTime date)
    {
        string quarterEnd = QuarterEndMonths.Single(q => q.LowerMonth <= date.Month && q.UpperMonth >= date.Month).QuarterEnd;
        return $"{quarterEnd}-{date.Year}";
    }

    private record class QuarterEndMonth(int LowerMonth, int UpperMonth, string QuarterEnd) { }
}
