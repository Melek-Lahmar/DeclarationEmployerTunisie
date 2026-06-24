using DeclarationEmployer.Contracts.Cabinet;
using FluentValidation;

namespace DeclarationEmployer.Application.Cabinet.Validation;

public sealed class UpdateFiscalYearRequestValidator : AbstractValidator<UpdateFiscalYearRequest>
{
    public UpdateFiscalYearRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);
    }
}
