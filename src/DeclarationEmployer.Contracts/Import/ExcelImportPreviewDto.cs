namespace DeclarationEmployer.Contracts.Import;

public sealed class ExcelImportPreviewDto
{
    public Guid DeclarationId { get; set; }

    public string TemporaryFileToken { get; set; } = string.Empty;

    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int InvalidRows { get; set; }

    public IReadOnlyList<ExcelImportRowDto> Rows { get; set; } = [];

    public IReadOnlyList<ExcelImportErrorDto> Errors { get; set; } = [];
}
