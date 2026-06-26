using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class UpdateDeclarationLineRequestValidator : AbstractValidator<UpdateDeclarationLineRequest>
{
    public UpdateDeclarationLineRequestValidator()
    {
        RuleFor(x => x.OperationType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FiscalCategory)
            .MaximumLength(100);

        RuleFor(x => x.GrossAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.TaxableAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Rate)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.WithheldAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DocumentReference)
            .MaximumLength(100);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50);
    }
}
