using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class UpdateDeclarationAnnexRequestValidator : AbstractValidator<UpdateDeclarationAnnexRequest>
{
    public UpdateDeclarationAnnexRequestValidator()
    {
        RuleFor(x => x.AnnexCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50);
    }
}
