using DeclarationEmployer.Contracts.Declarations;
using FluentValidation;

namespace DeclarationEmployer.Application.Declarations.Validation;

public sealed class ResolveDeclarationAnomalyRequestValidator : AbstractValidator<ResolveDeclarationAnomalyRequest>
{
    public ResolveDeclarationAnomalyRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(1000);
    }
}
