using DeclarationEmployer.FiscalEngine.Empcca;
using FluentAssertions;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaFixedWidthFormatterTests
{
    [Fact]
    public void FormatAlpha_PadsWithSpacesOnTheRight()
    {
        FixedWidthFormatter.FormatAlpha("ABC", 5).Should().Be("ABC  ");
        FixedWidthFormatter.FormatAlpha(null, 3).Should().Be("   ");
    }

    [Fact]
    public void FormatNumeric_PadsWithZerosOnTheLeft()
    {
        FixedWidthFormatter.FormatNumeric("25", 4).Should().Be("0025");
        FixedWidthFormatter.FormatNumeric(null, 3).Should().Be("000");
    }

    [Theory]
    [InlineData("1,25")]
    [InlineData("1.25")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1 2")]
    public void FormatNumeric_RejectsNonDigits(string value)
    {
        var action = () => FixedWidthFormatter.FormatNumeric(value, 10);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FormatInteger_RejectsNegativeValue()
    {
        var action = () => FixedWidthFormatter.FormatInteger(-1, 3);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, "000000000000000")]
    [InlineData(1.234, "000000000001234")]
    [InlineData(123.456, "000000000123456")]
    public void FormatAmountInMillimes_ProducesFifteenDigits(decimal amount, string expected)
    {
        FixedWidthFormatter.FormatAmountInMillimes(amount, 15).Should().Be(expected);
    }

    [Fact]
    public void FormatAmountInMillimes_RejectsFractionOfMillime()
    {
        var action = () => FixedWidthFormatter.FormatAmountInMillimes(1.2345m, 15);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FormatAmountInMillimes_RejectsNegativeAmount()
    {
        var action = () => FixedWidthFormatter.FormatAmountInMillimes(-0.001m, 15);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, "00000")]
    [InlineData(2.5, "00250")]
    [InlineData(15, "01500")]
    [InlineData(999.99, "99999")]
    public void FormatRate_UsesCobol999V99(decimal rate, string expected)
    {
        FixedWidthFormatter.FormatRate(rate).Should().Be(expected);
    }

    [Fact]
    public void FormatRate_RejectsMoreThanTwoDecimals()
    {
        var action = () => FixedWidthFormatter.FormatRate(2.555m);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FormatDate_UsesDdMmYyyy()
    {
        FixedWidthFormatter.FormatDate(new DateOnly(2025, 12, 3)).Should().Be("03122025");
    }

    [Theory]
    [InlineData("é")]
    [InlineData("\n")]
    [InlineData("\t")]
    [InlineData("\0")]
    public void EnsureAscii_RejectsNonPrintableAscii(string value)
    {
        var action = () => FixedWidthFormatter.EnsureAscii(value);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureExactLength_RejectsUnexpectedLength()
    {
        var action = () => FixedWidthFormatter.EnsureExactLength("ABC", 4);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Formatter_RejectsOverflowInsteadOfTruncating()
    {
        var alphaAction = () => FixedWidthFormatter.FormatAlpha("TOOLONG", 3);
        var numericAction = () => FixedWidthFormatter.FormatNumeric("1234", 3);

        alphaAction.Should().Throw<ArgumentException>();
        numericAction.Should().Throw<ArgumentException>();
    }
}
