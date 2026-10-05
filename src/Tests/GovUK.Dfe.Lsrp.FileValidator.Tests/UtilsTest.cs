using GovUK.Dfe.Lsrp.FileValidator.Models;
using GovUK.Dfe.Lsrp.FileValidator.Services;

namespace GovUK.Dfe.Lsrp.FileValidator.Tests;

public class UtilsTest
{
    [Theory]
    [InlineData("2026-27")]
    [InlineData("2000-01")]
    [InlineData("2098-99")]
    public void CheckYear_WhenYearRangeIsValid_ReturnsTrue(string yearRange)
    {
        yearRange += " Quarterly Data";
        bool result = Utils.CheckYear(yearRange);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2026-28")]
    [InlineData("26-27")]
    [InlineData("2026-2027")]
    [InlineData("2026/27")]
    public void CheckYear_WhenFormatIsInvalid_ReturnsFalse(string yearRange)
    {
        yearRange += " Quarterly Data";
        bool result = Utils.CheckYear(yearRange);

        Assert.False(result);
    }

    [Theory]
    [InlineData("2026-28")]
    [InlineData("2026-26")]
    [InlineData("2010-15")]
    public void CheckYear_WhenYearsAreNotConsecutive_ReturnsFalse(string yearRange)
    {
        yearRange += " Quarterly Data";
        bool result = Utils.CheckYear(yearRange);

        Assert.False(result);
    }

    [Fact]
    public void CheckYear_WhenValueIsNull_ReturnsFalse()
    {
        bool result = Utils.CheckYear(null!);

        Assert.False(result);
    }

    [Theory]
    [InlineData("LA1 Local Authority 1", "LA1", "Local-Authority-1")]
    [InlineData("LA1  Local Authority 1", "LA1", "Local-Authority-1")]
    [InlineData("LA1", "LA1", "")]
    public void CheckLocalAuthority_WhenLocalAuthoritiesMatch_ReturnsTrue(string actualLocalAuthority, string expectedCode, string expectedName)
    {
        LocalAuthority expectedLocalAuthority = new() { Code = expectedCode, Name = expectedName };
        bool result = Utils.CheckLocalAuthority(actualLocalAuthority, expectedLocalAuthority);

        Assert.True(result);
    }

    [Theory]
    [InlineData("LA2 Local Authority 2", "LA1", "Local-Authority-1")]
    [InlineData("LA1Local Authority 1", "LA1", "Local-Authority-1")]
    public void CheckLocalAuthority_WhenLocalAuthoritiesDoNotMatch_ReturnsFalse(string actualLocalAuthority, string expectedCode, string expectedName)
    {
        LocalAuthority expectedLocalAuthority = new() { Code = expectedCode, Name = expectedName };
        bool result = Utils.CheckLocalAuthority(actualLocalAuthority, expectedLocalAuthority);

        Assert.False(result);
    }
}
