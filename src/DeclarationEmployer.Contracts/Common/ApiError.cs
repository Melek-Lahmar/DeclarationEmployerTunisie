namespace DeclarationEmployer.Contracts.Common;

public sealed class ApiError
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public IDictionary<string, string[]>? Details { get; set; }

    public IDictionary<string, string[]>? ValidationErrors { get; set; }

    public string? TraceId { get; set; }
}
