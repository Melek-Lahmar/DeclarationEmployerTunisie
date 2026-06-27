using DeclarationEmployer.FiscalEngine.Empcca;
using FluentAssertions;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaFixedWidthRecordTests
{
    [Fact]
    public void Definition_RejectsGapBetweenFields()
    {
        var action = () => new FixedWidthRecordDefinition(
            "Test",
            "TST",
            5,
            [
                Field("Code", 1, 2, FixedWidthFieldType.AlphaNumeric),
                Field("Amount", 4, 5, FixedWidthFieldType.Numeric)
            ]);

        action.Should().Throw<ArgumentException>().WithMessage("*position 3*");
    }

    [Fact]
    public void Definition_RejectsIncompleteCoverage()
    {
        var action = () => new FixedWidthRecordDefinition(
            "Test",
            "TST",
            6,
            [Field("Value", 1, 5, FixedWidthFieldType.AlphaNumeric)]);

        action.Should().Throw<ArgumentException>().WithMessage("*5 caracteres au lieu de 6*");
    }

    [Fact]
    public void Builder_FormatsFieldsAndGuaranteesRecordLength()
    {
        var definition = new FixedWidthRecordDefinition(
            "Test",
            "TST",
            10,
            [
                Field("Code", 1, 2, FixedWidthFieldType.AlphaNumeric, required: true),
                Field("Name", 3, 6, FixedWidthFieldType.AlphaNumeric),
                Field("Amount", 7, 10, FixedWidthFieldType.Numeric, defaultValue: "0")
            ]);

        var record = new FixedWidthRecordBuilder().Build(
            definition,
            new Dictionary<string, string?>
            {
                ["Code"] = "L1",
                ["Name"] = "AB",
                ["Amount"] = "25"
            });

        record.Should().Be("L1AB  0025");
        record.Should().HaveLength(10);
    }

    [Fact]
    public void Builder_RejectsMissingRequiredField()
    {
        var definition = new FixedWidthRecordDefinition(
            "Test",
            "TST",
            2,
            [Field("Code", 1, 2, FixedWidthFieldType.AlphaNumeric, required: true)]);

        var action = () => new FixedWidthRecordBuilder().Build(
            definition,
            new Dictionary<string, string?>());

        action.Should().Throw<ArgumentException>().WithMessage("*Code*absent*");
    }

    private static FixedWidthFieldDefinition Field(
        string name,
        int start,
        int end,
        FixedWidthFieldType type,
        bool required = false,
        string? defaultValue = null)
    {
        return new FixedWidthFieldDefinition
        {
            Name = name,
            StartPosition = start,
            EndPosition = end,
            Type = type,
            Required = required,
            DefaultValue = defaultValue
        };
    }
}
