namespace DeclarationEmployer.Contracts.Validation;

public sealed class ValidationRunSummaryDto
{
    public ValidationRunDto Run { get; set; } = new();

    public IReadOnlyList<ValidationResultDto> Results { get; set; } = Array.Empty<ValidationResultDto>();
}
