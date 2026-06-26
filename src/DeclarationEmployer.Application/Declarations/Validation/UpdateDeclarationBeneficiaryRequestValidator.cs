using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class UpdateDeclarationBeneficiaryRequestValidator : AbstractValidator<UpdateDeclarationBeneficiaryRequest>
{
    public UpdateDeclarationBeneficiaryRequestValidator()
    {
        RuleFor(x => x.IdentifierType)
            .NotEmpty();

        RuleFor(x => x.Identifier)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.FullNameOrCompanyName)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Address)
            .MaximumLength(500);

        RuleFor(x => x.Country)
            .MaximumLength(100);
    }
}
