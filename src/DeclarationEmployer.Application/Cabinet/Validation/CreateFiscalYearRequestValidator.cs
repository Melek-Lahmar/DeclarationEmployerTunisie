using DeclarationEmployer.Contracts.Cabinet;
using FluentValidation;

namespace DeclarationEmployer.Application.Cabinet.Validation;

public sealed class CreateFiscalYearRequestValidator : AbstractValidator<CreateFiscalYearRequest>
{
    public CreateFiscalYearRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);
    }
}
