namespace DeclarationEmployer.Contracts.Declarations;

public sealed class DeclarationAnnexDto
{
    public Guid Id { get; set; }

    public Guid DeclarationId { get; set; }

    public string AnnexCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
