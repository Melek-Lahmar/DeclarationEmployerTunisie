using System.Text;
using DeclarationEmployer.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class DeclarationExportStorageService : IDeclarationExportStorageService
{
    private readonly StorageOptions _options;
    private readonly IHostEnvironment _environment;

    public DeclarationExportStorageService(
        IOptions<StorageOptions> options,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public async Task<(string RelativePath, string AbsolutePath)> SaveExportAsync(
        string clientCode,
        int year,
        string fileName,
        string content,
        CancellationToken cancellationToken = default)
    {
        var sanitizedClientCode = SanitizePathSegment(clientCode);
        var sanitizedFileName = Path.GetFileName(fileName);
        var relativePath = BuildExportRelativePath(sanitizedClientCode, year, sanitizedFileName);
        var absolutePath = Path.Combine(GetRootPath(), relativePath);
        var directory = Path.GetDirectoryName(absolutePath)!;

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(absolutePath, content, Encoding.UTF8, cancellationToken);

        return (relativePath.Replace('\\', '/'), absolutePath);
    }

    public string BuildExportRelativePath(string clientCode, int year, string fileName)
    {
        var sanitizedClientCode = SanitizePathSegment(clientCode);
        var sanitizedFileName = Path.GetFileName(fileName);

        return Path.Combine("clients", sanitizedClientCode, year.ToString(), "exports", sanitizedFileName);
    }

    private string GetRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.RootPath))
        {
            return _options.RootPath;
        }

        return _environment.IsDevelopment()
            ? "C:/DET2025_DEV/storage"
            : Path.Combine(AppContext.BaseDirectory, "storage");
    }

    private static string SanitizePathSegment(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(
            value
                .Trim()
                .Select(ch => invalidChars.Contains(ch) || char.IsWhiteSpace(ch) ? '_' : ch)
                .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "CLIENT" : sanitized;
    }
}
