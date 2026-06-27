using DeclarationEmployer.Contracts.Declarations.Empcca;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class EmpccaBeneficiaryInputValidator : AbstractValidator<EmpccaBeneficiaryInput>
{
    public EmpccaBeneficiaryInputValidator()
    {
        RuleFor(x => x.IdentifierType).InclusiveBetween(1, 4);
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(13);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Activity).MaximumLength(40);
        RuleFor(x => x.JobTitle).MaximumLength(40);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(120);
    }
}

public abstract class EmpccaAnnexRequestValidatorBase<T> : AbstractValidator<T>
{
    protected void ConfigureCommon(
        System.Linq.Expressions.Expression<Func<T, int>> orderNumber,
        System.Linq.Expressions.Expression<Func<T, EmpccaBeneficiaryInput>> beneficiary)
    {
        RuleFor(orderNumber).InclusiveBetween(1, 999999);
        RuleFor(beneficiary).NotNull().SetValidator(new EmpccaBeneficiaryInputValidator());
    }

    protected void NonNegative(params System.Linq.Expressions.Expression<Func<T, decimal>>[] fields)
    {
        foreach (var field in fields)
        {
            RuleFor(field).GreaterThanOrEqualTo(0m).PrecisionScale(18, 3, true);
        }
    }
}

public sealed class CreateEmpccaAnnexA1LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA1LineRequest>
{
    public CreateEmpccaAnnexA1LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        RuleFor(x => x.Beneficiary.IdentifierType).Equal(2).WithMessage("L'annexe A1 exige un CIN.");
        RuleFor(x => x.FamilySituation).InclusiveBetween(1, 4);
        RuleFor(x => x.DependentChildrenCount).InclusiveBetween(0, 99);
        RuleFor(x => x.WorkPeriodStart).NotEmpty();
        RuleFor(x => x.WorkPeriodEnd).GreaterThanOrEqualTo(x => x.WorkPeriodStart);
        RuleFor(x => x.WorkPeriodDays).InclusiveBetween(0, 999);
        NonNegative(x => x.TaxableIncome, x => x.BenefitsInKind, x => x.GrossTaxableIncome,
            x => x.ReinvestedIncome, x => x.CommonRegimeWithheldAmount,
            x => x.ForeignEmployeeWithheldAmount, x => x.SocialSolidarityContribution, x => x.NetPaidAmount);
    }
}

public sealed class CreateEmpccaAnnexA2LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA2LineRequest>
{
    public CreateEmpccaAnnexA2LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        RuleFor(x => x.Beneficiary.IdentifierType).InclusiveBetween(1, 2);
        RuleFor(x => x.AmountType).InclusiveBetween(0, 6);
        NonNegative(x => x.GrossProfessionalAmount, x => x.RealRegimeFeesAmount,
            x => x.BoardAndSecuritiesAmount, x => x.OccasionalWorkAmount,
            x => x.RealEstateCapitalGainAmount, x => x.HotelRentAmount,
            x => x.ArtistRemunerationAmount, x => x.PublicSectorVatWithheldAmount,
            x => x.WithheldAmount, x => x.NetPaidAmount);
    }
}

public sealed class CreateEmpccaAnnexA5LineRequestValidator : EmpccaAnnexRequestValidatorBase<CreateEmpccaAnnexA5LineRequest>
{
    public CreateEmpccaAnnexA5LineRequestValidator()
    {
        ConfigureCommon(x => x.OrderNumber, x => x.Beneficiary);
        NonNegative(x => x.PurchasesFromTenPercentCompanies, x => x.PurchasesFromFifteenPercentCompanies,
            x => x.PurchasesFromTwoThirdsDeductionBusinesses, x => x.PurchasesFromOtherBusinesses,
            x => x.VatWithheldAmount, x => x.DeliveryPlatformThreePercentWithheldAmount,
            x => x.WithheldAmount, x => x.NetPaidAmount);
    }
}
