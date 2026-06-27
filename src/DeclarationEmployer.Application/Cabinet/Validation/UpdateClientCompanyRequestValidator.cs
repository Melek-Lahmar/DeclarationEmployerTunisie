using DeclarationEmployer.Contracts.Cabinet;
using FluentValidation;

namespace DeclarationEmployer.Application.Cabinet.Validation;

public sealed class UpdateClientCompanyRequestValidator : AbstractValidator<UpdateClientCompanyRequest>
{
    public UpdateClientCompanyRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RaisonSociale)
            .NotEmpty()
            .MaximumLength(250);

        RuleFor(x => x.MatriculeFiscal).MaximumLength(20);
        RuleFor(x => x.Cle).MaximumLength(5);
        RuleFor(x => x.Categorie).MaximumLength(5);
        RuleFor(x => x.CodeTva).MaximumLength(5);
        RuleFor(x => x.Etablissement).MaximumLength(10);
        RuleFor(x => x.Activite).MaximumLength(250);
        RuleFor(x => x.Adresse).MaximumLength(500);
        RuleFor(x => x.Ville).MaximumLength(100);
        RuleFor(x => x.NumeroAdresse).MaximumLength(20);
        RuleFor(x => x.CodePostal).MaximumLength(20);
        RuleFor(x => x.Telephone).MaximumLength(50);
    }
}
