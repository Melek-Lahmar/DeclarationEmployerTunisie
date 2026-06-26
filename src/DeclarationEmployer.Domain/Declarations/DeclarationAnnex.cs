namespace DeclarationEmployer.Domain.Declarations;

public sealed class DeclarationAnnex
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeclarationId { get; set; }

    public string AnnexCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DeclarationAnnexStatus Status { get; set; } = DeclarationAnnexStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public EmployerDeclaration? Declaration { get; set; }
}
