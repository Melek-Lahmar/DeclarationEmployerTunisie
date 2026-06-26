using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class CreateDeclarationAnnexRequestValidator : AbstractValidator<CreateDeclarationAnnexRequest>
{
    public CreateDeclarationAnnexRequestValidator()
    {
        RuleFor(x => x.AnnexCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(250);
    }
}
