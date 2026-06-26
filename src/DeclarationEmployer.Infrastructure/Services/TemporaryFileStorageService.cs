using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class TemporaryFileStorageService : ITemporaryFileStorageService
{
    private readonly StorageOptions _options;
    private readonly IHostEnvironment _environment;

    public TemporaryFileStorageService(
        IOptions<StorageOptions> options,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<string> SaveTemporaryImportFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        EnsureExcelExtension(fileName);
        var token = $"{Guid.NewGuid():N}.xlsx";
        var path = BuildFilePath(token);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = File.Create(path);
        fileStream.Position = 0;
        await fileStream.CopyToAsync(output, cancellationToken);
        return token;
    }

    public Task<string> GetTemporaryImportFilePathAsync(
        string temporaryFileToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(temporaryFileToken) ||
            !temporaryFileToken.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApplicationConflictException("Le token du fichier temporaire est invalide.");
        }

        var path = BuildFilePath(Path.GetFileName(temporaryFileToken));
        if (!File.Exists(path))
        {
            throw new ApplicationNotFoundException("Le fichier temporaire d'import est introuvable.");
        }

        return Task.FromResult(path);
    }

    public Task DeleteTemporaryImportFileAsync(
        string temporaryFileToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = BuildFilePath(Path.GetFileName(temporaryFileToken));
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string BuildFilePath(string token)
    {
        var rootPath = string.IsNullOrWhiteSpace(_options.RootPath)
            ? (_environment.IsDevelopment() ? "C:/DET2025_DEV/storage" : Path.Combine(AppContext.BaseDirectory, "storage"))
            : _options.RootPath;

        return Path.Combine(rootPath, "temp", "imports", token);
    }

    private static void EnsureExcelExtension(string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApplicationConflictException("Seuls les fichiers .xlsx sont autorises.");
        }
    }
}
