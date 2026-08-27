using GovUK.Dfe.Lsrp.FileValidator.Services;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class UtilsTest
{
    [Theory]
    [InlineData("2026-27 Data")]
    [InlineData("2000-01 Data")]
    [InlineData("2098-99 Data")]
    public void CheckYear_WhenYearRangeIsValid_ReturnsTrue(string yearRange)
    {
        bool result = Utils.CheckYear(yearRange);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2026-27")]
    [InlineData("26-27 Data")]
    [InlineData("2026-2027 Data")]
    [InlineData("2026/27 Data")]
    [InlineData("2026-27Data")]
    public void CheckYear_WhenFormatIsInvalid_ReturnsFalse(string yearRange)
    {
        bool result = Utils.CheckYear(yearRange);

        Assert.False(result);
    }

    [Theory]
    [InlineData("2026-28 Data")]
    [InlineData("2026-26 Data")]
    [InlineData("2010-15 Data")]
    public void CheckYear_WhenYearsAreNotConsecutive_ReturnsFalse(string yearRange)
    {
        bool result = Utils.CheckYear(yearRange);

        Assert.False(result);
    }

    [Fact]
    public void CheckYear_WhenValueIsNull_ReturnsFalse()
    {
        bool result = Utils.CheckYear(null!);

        Assert.False(result);
    }
}
