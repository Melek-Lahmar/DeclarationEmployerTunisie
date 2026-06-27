using DeclarationEmployer.Contracts.Cabinet;
using FluentValidation;

namespace DeclarationEmployer.Application.Cabinet.Validation;

public sealed class CreateClientCompanyRequestValidator : AbstractValidator<CreateClientCompanyRequest>
{
    public CreateClientCompanyRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Le code société est obligatoire.")
            .MaximumLength(20).WithMessage("Le code société ne doit pas dépasser 20 caractères.");

        RuleFor(x => x.RaisonSociale)
            .NotEmpty().WithMessage("La raison sociale est obligatoire.")
            .MaximumLength(200).WithMessage("La raison sociale ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.MatriculeFiscal).NotEmpty().WithMessage("L'identifiant fiscal est obligatoire.")
            .Matches("^[0-9]{7}$").WithMessage("L'identifiant fiscal doit contenir 7 chiffres.");
        RuleFor(x => x.Cle).NotEmpty().WithMessage("La clef fiscale est obligatoire.").Length(1);
        RuleFor(x => x.Categorie).NotEmpty().WithMessage("La catégorie contribuable est obligatoire.").Length(1)
            .Must(value => !string.Equals(value?.Trim(), "E", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La catégorie contribuable ne doit pas être E.");
        RuleFor(x => x.CodeTva).MaximumLength(1);
        RuleFor(x => x.Etablissement)
            .Must(value => string.IsNullOrWhiteSpace(value) || System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), "^[0-9]{3}$"))
            .WithMessage("Le numéro d'établissement doit contenir 3 chiffres.");
        RuleFor(x => x.Activite).NotEmpty().WithMessage("L'activité est obligatoire.").MaximumLength(250);
        RuleFor(x => x.Adresse).NotEmpty().WithMessage("La rue est obligatoire.").MaximumLength(500);
        RuleFor(x => x.Ville).NotEmpty().WithMessage("La ville est obligatoire.").MaximumLength(100);
        RuleFor(x => x.NumeroAdresse).Matches("^[0-9]+$").When(x => !string.IsNullOrWhiteSpace(x.NumeroAdresse))
            .WithMessage("Le numéro de rue doit être numérique.").MaximumLength(20);
        RuleFor(x => x.CodePostal).Matches("^[0-9]{4}$").When(x => !string.IsNullOrWhiteSpace(x.CodePostal))
            .WithMessage("Le code postal doit contenir 4 chiffres.");
        RuleFor(x => x.Telephone).Matches("^[0-9+() ./-]+$").When(x => !string.IsNullOrWhiteSpace(x.Telephone))
            .WithMessage("Le téléphone contient un format invalide.").MaximumLength(50);
    }
}
