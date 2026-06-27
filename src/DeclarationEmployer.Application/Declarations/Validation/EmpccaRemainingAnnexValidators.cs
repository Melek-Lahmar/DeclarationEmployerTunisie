using DeclarationEmployer.Contracts.Declarations.Empcca;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class CreateEmpccaAnnexA3LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA3LineRequest>
{
    public CreateEmpccaAnnexA3LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        NonNegative(x => x.SavingsAccountInterest, x => x.OtherMovableCapitalIncome,
            x => x.NonEstablishedBankLoanInterest, x => x.WithheldAmount, x => x.NetPaidAmount);
    }
}

public sealed class CreateEmpccaAnnexA4LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA4LineRequest>
{
    public CreateEmpccaAnnexA4LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        RuleFor(x => x.Beneficiary.IdentifierType).InclusiveBetween(3, 4);
        RuleFor(x => x.AmountType).InclusiveBetween(0, 9);
        NonNegative(x => x.ProfessionalAmountRate, x => x.ProfessionalAmount, x => x.ConstructionWorkRate,
            x => x.ConstructionWorkAmount, x => x.RealEstateCapitalGainRate, x => x.RealEstateCapitalGainAmount,
            x => x.SecuritiesCapitalGainRate, x => x.SecuritiesCapitalGainAmount, x => x.SecuritiesIncomeRate,
            x => x.SecuritiesIncomeAmount, x => x.PrivilegedTaxRegimeAmount, x => x.VatWithheldAmount,
            x => x.WithheldAmount, x => x.NetPaidAmount);
        RuleFor(x => x.ProfessionalAmountRate).LessThanOrEqualTo(999.99m);
        RuleFor(x => x.ConstructionWorkRate).LessThanOrEqualTo(999.99m);
        RuleFor(x => x.RealEstateCapitalGainRate).LessThanOrEqualTo(999.99m);
        RuleFor(x => x.SecuritiesCapitalGainRate).LessThanOrEqualTo(999.99m);
        RuleFor(x => x.SecuritiesIncomeRate).LessThanOrEqualTo(999.99m);
    }
}

public sealed class CreateEmpccaAnnexA6LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA6LineRequest>
{
    public CreateEmpccaAnnexA6LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        RuleFor(x => x.RebateType).InclusiveBetween(0, 2);
        RuleFor(x => x).Must(x => x.RebateAmount == 0 || x.RebateType is 1 or 2)
            .WithMessage("Une ristourne non nulle exige le type 1 ou 2.");
        NonNegative(x => x.RebateAmount, x => x.FlatRegimeSalesAmount, x => x.FlatRegimeSalesAdvanceAmount,
            x => x.GamblingIncomeAmount, x => x.GamblingWithheldAmount, x => x.DistributionNetworkSalesAmount,
            x => x.DistributionNetworkWithheldAmount, x => x.CashCollectionsAmount, x => x.AlcoholSalesAmount,
            x => x.AlcoholSalesAdvanceAmount);
    }
}

public sealed class CreateEmpccaAnnexA7LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA7LineRequest>
{
    private static readonly int[] AllowedTypes = [1, 2, 3, 4, 5, 6, 7, 8, 15, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29];

    public CreateEmpccaAnnexA7LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        RuleFor(x => x.PaidAmountType).Must(x => AllowedTypes.Contains(x));
        NonNegative(x => x.PaidAmount, x => x.WithheldAmount, x => x.NetPaidAmount);
    }
}
