namespace DeclarationEmployer.Domain.Declarations;

public sealed class ValidationResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ValidationRunId { get; set; }

    public Guid DeclarationId { get; set; }

    public string? AnnexCode { get; set; }

    public string? LineId { get; set; }

    public DeclarationAnomalySeverity Severity { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? FieldName { get; set; }

    public ValidationResultStatus Status { get; set; } = ValidationResultStatus.Open;

    public string? Justification { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }

    public ValidationRun? ValidationRun { get; set; }

    public EmployerDeclaration? Declaration { get; set; }
}
