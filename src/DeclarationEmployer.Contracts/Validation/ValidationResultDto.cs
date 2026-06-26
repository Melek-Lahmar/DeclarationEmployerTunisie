namespace DeclarationEmployer.Contracts.Validation;

public sealed class ValidationResultDto
{
    public Guid Id { get; set; }

    public Guid ValidationRunId { get; set; }

    public Guid DeclarationId { get; set; }

    public string? AnnexCode { get; set; }

    public string? LineId { get; set; }

    public string Severity { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? FieldName { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Justification { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}
