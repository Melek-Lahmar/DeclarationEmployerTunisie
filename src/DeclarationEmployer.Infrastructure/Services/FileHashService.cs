using System.Security.Cryptography;
using DeclarationEmployer.Application.Common;

namespace DeclarationEmployer.Infrastructure.Services;

public sealed class FileHashService : IFileHashService
{
    public async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new ApplicationNotFoundException("Le fichier a hasher est introuvable.");
        }

        await using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}
