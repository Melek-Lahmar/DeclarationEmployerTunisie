namespace DeclarationEmployer.Infrastructure.Services;

public interface ITemporaryFileStorageService
{
    Task<string> SaveTemporaryImportFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<string> GetTemporaryImportFilePathAsync(
        string temporaryFileToken,
        CancellationToken cancellationToken = default);

    Task DeleteTemporaryImportFileAsync(
        string temporaryFileToken,
        CancellationToken cancellationToken = default);
}
