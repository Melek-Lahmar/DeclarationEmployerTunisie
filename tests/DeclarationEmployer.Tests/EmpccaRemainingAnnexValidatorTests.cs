using DeclarationEmployer.Application.Declarations.Validation;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using FluentAssertions;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaRemainingAnnexValidatorTests
{
    [Fact]
    public void AnnexA4Validator_RejectsResidentIdentifierTypeAndOversizedRate()
    {
        var request = new CreateEmpccaAnnexA4LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            AmountType = 1,
            ProfessionalAmountRate = 1000
        };
        var result = new CreateEmpccaAnnexA4LineRequestValidator().Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AnnexA6Validator_RejectsNonZeroRebateWithTypeZero()
    {
        var request = new CreateEmpccaAnnexA6LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            RebateType = 0,
            RebateAmount = 100
        };
        var result = new CreateEmpccaAnnexA6LineRequestValidator().Validate(request);
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("ristourne non nulle", StringComparison.Ordinal));
    }

    [Fact]
    public void AnnexA7Validator_AcceptsDocumentedType29ButRejectsUnknownType()
    {
        var validator = new CreateEmpccaAnnexA7LineRequestValidator();
        var request = new CreateEmpccaAnnexA7LineRequest { OrderNumber = 1, Beneficiary = Beneficiary(1), PaidAmountType = 29 };
        validator.Validate(request).IsValid.Should().BeTrue();
        request.PaidAmountType = 18;
        validator.Validate(request).IsValid.Should().BeFalse();
    }

    private static EmpccaBeneficiaryInput Beneficiary(int type) => new()
    {
        IdentifierType = type,
        Identifier = "1234567A/B000",
        Name = "BENEFICIARY",
        Address = "TUNIS"
    };
}
