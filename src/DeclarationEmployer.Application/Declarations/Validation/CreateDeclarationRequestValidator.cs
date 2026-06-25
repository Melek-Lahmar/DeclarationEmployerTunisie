using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class CreateDeclarationRequestValidator : AbstractValidator<CreateDeclarationRequest>
{
    public CreateDeclarationRequestValidator()
    {
        RuleFor(x => x.ClientCompanyId)
            .NotEmpty();

        RuleFor(x => x.FiscalYearId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(250);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
