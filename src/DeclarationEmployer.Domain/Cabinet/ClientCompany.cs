namespace DeclarationEmployer.Domain.Cabinet;

public sealed class ClientCompany
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string RaisonSociale { get; set; } = string.Empty;

    public string? MatriculeFiscal { get; set; }

    public string? Cle { get; set; }

    public string? Categorie { get; set; }

    public string? CodeTva { get; set; }

    public string? Etablissement { get; set; }

    public string? Activite { get; set; }

    public string? Adresse { get; set; }

    public string? Ville { get; set; }

    public string? NumeroAdresse { get; set; }

    public string? CodePostal { get; set; }

    public string? Telephone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
