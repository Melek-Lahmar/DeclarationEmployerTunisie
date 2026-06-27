namespace DeclarationEmployer.Contracts.Cabinet;

public sealed class ClientCompanyDto
{
    public Guid Id { get; set; }

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

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
