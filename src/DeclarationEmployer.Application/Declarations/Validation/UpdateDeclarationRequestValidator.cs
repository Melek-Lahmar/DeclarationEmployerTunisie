using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class UpdateDeclarationRequestValidator : AbstractValidator<UpdateDeclarationRequest>
{
    public UpdateDeclarationRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
