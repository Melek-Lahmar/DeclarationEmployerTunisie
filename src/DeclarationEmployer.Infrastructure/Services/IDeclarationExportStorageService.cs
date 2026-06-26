namespace DeclarationEmployer.Infrastructure.Services;

public interface IDeclarationExportStorageService
{
    Task<(string RelativePath, string AbsolutePath)> SaveExportAsync(
        string clientCode,
        int year,
        string fileName,
        string content,
        CancellationToken cancellationToken = default);

    string BuildExportRelativePath(string clientCode, int year, string fileName);
}
