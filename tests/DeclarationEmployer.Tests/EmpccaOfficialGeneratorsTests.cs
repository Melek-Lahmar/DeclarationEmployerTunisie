using DeclarationEmployer.Domain.Declarations.Empcca;
using DeclarationEmployer.FiscalEngine.Empcca;
using FluentAssertions;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaOfficialGeneratorsTests
{
    [Fact]
    public void Annex1Generator_ProducesHeaderLinesAndReconciledTotalAt399Characters()
    {
        var records = new[]
        {
            new EmpccaAnnexA1Record(1, CinBeneficiary("12345678", "EMPLOYEE ONE"), A1Detail(1000, 100, 900)),
            new EmpccaAnnexA1Record(2, CinBeneficiary("87654321", "EMPLOYEE TWO"), A1Detail(500, 50, 450))
        };
        var result = new OfficialAnnex1Generator().Generate(Declarant(), records);

        result.FileName.Should().Be("ANXEMP_1_25_1");
        result.Lines.Should().HaveCount(4).And.OnlyContain(x => x.Length == 399);
        result.Lines[0].Should().StartWith("E1");
        result.Lines[1].Should().StartWith("L1");
        result.Lines[^1].Should().StartWith("T1");
        result.Lines[^1].Substring(320, 15).Should().Be("000000000150000");
        result.IsOfficial.Should().BeFalse();
    }

    [Fact]
    public void Annex2Generator_ProducesExactLengthAndTotals()
    {
        var detail = new AnnexA2Detail
        {
            AmountType = 1,
            GrossProfessionalAmount = 1200,
            WithheldAmount = 180,
            NetPaidAmount = 1020
        };
        var result = new OfficialAnnex2Generator().Generate(
            Declarant(), [new EmpccaAnnexA2Record(1, FiscalBeneficiary(), detail)]);

        result.Lines.Should().HaveCount(3).And.OnlyContain(x => x.Length == 399);
        result.Lines[^1].Should().StartWith("T2");
        result.Lines[^1].Substring(359, 15).Should().Be("000000000180000");
    }

    [Fact]
    public void Annex5Generator_IncludesDeliveryPlatformThreePercentAmount()
    {
        var detail = new AnnexA5Detail
        {
            PurchasesFromFifteenPercentCompanies = 2000,
            DeliveryPlatformThreePercentWithheldAmount = 60,
            WithheldAmount = 60,
            NetPaidAmount = 1940
        };
        var result = new OfficialAnnex5Generator().Generate(
            Declarant(), [new EmpccaAnnexA5Record(1, FiscalBeneficiary(), detail)]);

        result.Lines.Should().HaveCount(3).And.OnlyContain(x => x.Length == 399);
        result.Lines[1].Substring(313, 15).Should().Be("000000000060000");
        result.Lines[^1].Substring(313, 15).Should().Be("000000000060000");
    }

    [Fact]
    public void DecempGenerator_ProducesExactly51AsciiLinesOf38CharactersAndFooterTotal()
    {
        var values = new Dictionary<int, EmpccaDecempRecordValue>
        {
            [1] = new(1000, 10, 100),
            [49] = new(2000, 3, 60)
        };
        var result = new OfficialDecemp2025Generator().Generate(Declarant(), new HashSet<int> { 1, 5 }, values);

        result.FileName.Should().Be("DECEMP_25");
        result.Lines.Should().HaveCount(51).And.OnlyContain(x => x.Length == 38);
        result.Lines.Should().OnlyContain(x => x.All(character => character >= (char)32 && character <= (char)126));
        result.Lines[0].Substring(19, 7).Should().Be("0111011");
        result.Lines[^1].Should().Be("999" + new string(' ', 20) + "000000000160000");
        result.IsOfficial.Should().BeFalse();
        result.BlockingIssues.Should().NotBeEmpty();
    }

    [Fact]
    public void OfficialGenerationGuard_BlocksPreviewWithRegulatoryAmbiguities()
    {
        var result = new OfficialDecemp2025Generator().Generate(
            Declarant(), new HashSet<int>(), new Dictionary<int, EmpccaDecempRecordValue>());

        var action = () => new EmpccaOfficialGenerationGuard().EnsureOfficialGenerationAllowed([result]);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Generation officielle bloquee*");
    }

    [Fact]
    public void RemainingAnnexGenerators_ProduceThreeRecordsOf399Characters()
    {
        var beneficiary = FiscalBeneficiary();
        var a3 = new OfficialAnnex3Generator().Generate(Declarant(),
            [new EmpccaAnnexA3Record(1, beneficiary, new AnnexA3Detail { SavingsAccountInterest = 100, WithheldAmount = 10, NetPaidAmount = 90 })]);
        var a4 = new OfficialAnnex4Generator().Generate(Declarant(),
            [new EmpccaAnnexA4Record(1, beneficiary, new AnnexA4Detail { AmountType = 1, ProfessionalAmountRate = 10, ProfessionalAmount = 100, WithheldAmount = 10, NetPaidAmount = 90 })]);
        var a6 = new OfficialAnnex6Generator().Generate(Declarant(),
            [new EmpccaAnnexA6Record(1, beneficiary, new AnnexA6Detail { RebateType = 1, RebateAmount = 100 })]);
        var a7 = new OfficialAnnex7Generator().Generate(Declarant(),
            [new EmpccaAnnexA7Record(1, beneficiary, new AnnexA7Detail { PaidAmountType = 2, PaidAmount = 100, WithheldAmount = 10, NetPaidAmount = 90 })]);

        new[] { a3, a4, a6, a7 }.Should().OnlyContain(result =>
            result.Lines.Count == 3 && result.Lines.All(line => line.Length == 399));
        a3.Lines[^1].Substring(283, 15).Should().Be("000000000010000");
        a4.Lines[^1].Substring(369, 15).Should().Be("000000000010000");
        a6.Lines[^1].Substring(239, 15).Should().Be("000000000100000");
        a7.Lines[^1].Substring(255, 15).Should().Be("000000000010000");
    }

    [Fact]
    public void Annex7Generator_Type29RemainsExplicitlyBlocked()
    {
        var result = new OfficialAnnex7Generator().Generate(Declarant(),
            [new EmpccaAnnexA7Record(1, FiscalBeneficiary(), new AnnexA7Detail { PaidAmountType = 29 })]);

        result.BlockingIssues.Should().Contain(x => x.Contains("type A7 29", StringComparison.Ordinal));
        result.IsOfficial.Should().BeFalse();
    }

    private static EmpccaDeclarant Declarant() => new(
        "1234567", "A", "B", "000", 2025, 0,
        "COMPANY", "ACCOUNTING", "TUNIS", "MAIN STREET", "10", "1000");

    private static EmpccaBeneficiary CinBeneficiary(string identifier, string name) =>
        new(2, identifier, name, "ACCOUNTANT", "TUNIS");

    private static EmpccaBeneficiary FiscalBeneficiary() =>
        new(1, "7654321A/B000", "SUPPLIER", "SERVICES", "TUNIS");

    private static AnnexA1Detail A1Detail(decimal gross, decimal withheld, decimal net) => new()
    {
        FamilySituation = 2,
        DependentChildrenCount = 1,
        WorkPeriodStart = new DateOnly(2025, 1, 1),
        WorkPeriodEnd = new DateOnly(2025, 12, 31),
        WorkPeriodDays = 365,
        TaxableIncome = gross,
        GrossTaxableIncome = gross,
        CommonRegimeWithheldAmount = withheld,
        NetPaidAmount = net
    };
}
