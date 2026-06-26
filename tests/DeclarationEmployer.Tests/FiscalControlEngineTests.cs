using DeclarationEmployer.FiscalEngine;
using DeclarationEmployer.FiscalEngine.Rules;
using FluentAssertions;

namespace DeclarationEmployer.Tests;

public sealed class FiscalControlEngineTests
{
    [Fact]
    public void FiscalControlEngine_WithValidLines_ReturnsNoBlockingIssues()
    {
        var engine = CreateEngine();
        var result = engine.Run(CreateContext(CreateValidLine()));

        result.BlockingIssuesCount.Should().Be(0);
    }

    [Fact]
    public void FiscalControlEngine_NegativeGrossAmount_ReturnsBlockingIssue()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.GrossAmount = -1m;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_GROSS_AMOUNT_NEGATIVE" && x.Severity == FiscalControlSeverity.Blocking);
    }

    [Fact]
    public void FiscalControlEngine_InvalidRate_ReturnsBlockingIssue()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.Rate = 101m;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_RATE_INVALID");
    }

    [Fact]
    public void FiscalControlEngine_WithheldGreaterThanTaxable_ReturnsBlockingIssue()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.WithheldAmount = 150m;
        line.TaxableAmount = 100m;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_WITHHELD_EXCEEDS_TAXABLE");
    }

    [Fact]
    public void FiscalControlEngine_MissingBeneficiary_ReturnsBlockingIssue()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.BeneficiaryId = null;
        line.BeneficiaryIdentifier = null;
        line.BeneficiaryName = null;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_BENEFICIARY_REQUIRED");
    }

    [Fact]
    public void FiscalControlEngine_PaymentDateOutsideFiscalYear_ReturnsBlockingIssue()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.PaymentDate = new DateTime(2024, 12, 31);

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_PAYMENT_DATE_OUT_OF_YEAR");
    }

    [Fact]
    public void FiscalControlEngine_ZeroRate_ReturnsWarning()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.Rate = 0m;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_RATE_ZERO" && x.Severity == FiscalControlSeverity.Warning);
    }

    [Fact]
    public void FiscalControlEngine_MissingDocumentReference_ReturnsWarning()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.DocumentReference = null;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_DOCUMENT_REFERENCE_MISSING");
    }

    [Fact]
    public void FiscalControlEngine_MissingFiscalCategory_ReturnsInfo()
    {
        var engine = CreateEngine();
        var line = CreateValidLine();
        line.FiscalCategory = null;

        var result = engine.Run(CreateContext(line));

        result.Issues.Should().Contain(x => x.Code == "LINE_FISCAL_CATEGORY_MISSING" && x.Severity == FiscalControlSeverity.Info);
    }

    private static FiscalControlContext CreateContext(params FiscalControlLine[] lines)
    {
        return new FiscalControlContext
        {
            DeclarationId = Guid.NewGuid(),
            FiscalYear = 2025,
            Lines = lines
        };
    }

    private static FiscalControlLine CreateValidLine()
    {
        return new FiscalControlLine
        {
            LineId = Guid.NewGuid(),
            BeneficiaryId = Guid.NewGuid(),
            BeneficiaryIdentifier = "12345678",
            BeneficiaryName = "Ali Test",
            OperationType = "Honoraires",
            FiscalCategory = "BNC",
            GrossAmount = 100m,
            TaxableAmount = 100m,
            Rate = 10m,
            WithheldAmount = 10m,
            PaymentDate = new DateTime(2025, 3, 31),
            DocumentReference = "DOC-001"
        };
    }

    private static IFiscalControlEngine CreateEngine()
    {
        return new FiscalControlEngine(
        [
            new GrossAmountMustBePositiveOrZeroRule(),
            new TaxableAmountMustBePositiveOrZeroRule(),
            new WithheldAmountMustBePositiveOrZeroRule(),
            new RateMustBeBetweenZeroAndHundredRule(),
            new WithheldAmountMustNotExceedTaxableAmountRule(),
            new BeneficiaryRequiredRule(),
            new OperationTypeRequiredRule(),
            new PaymentDateMustBeInsideFiscalYearRule(),
            new ZeroRateWarningRule(),
            new MissingDocumentReferenceWarningRule(),
            new ZeroTaxableWithWithheldAmountWarningRule(),
            new MissingFiscalCategoryInfoRule()
        ]);
    }
}
