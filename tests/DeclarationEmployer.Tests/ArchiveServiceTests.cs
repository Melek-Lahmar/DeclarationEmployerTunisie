using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Tests;

public sealed class ArchiveServiceTests
{
    [Fact]
    public async Task ArchiveAsync_CreatesReceiptAndLocksDeclaration()
    {
        using var fixture = CreateFixture();
        var declarationId = await SeedDeclarationAsync(fixture.Db);

        var result = await fixture.Service.ArchiveAsync(declarationId);

        result.DeclarationStatus.Should().Be(DeclarationStatus.Archived.ToString());
        result.Sha256Hash.Should().NotBeNullOrWhiteSpace();
        fixture.Db.ArchivedDocuments.Should().ContainSingle(x => x.Id == result.ArchivedDocumentId);
        fixture.Db.Declarations.Single().IsLocked.Should().BeTrue();
        File.Exists(Path.Combine(fixture.RootPath, result.RelativePath.Replace('/', Path.DirectorySeparatorChar))).Should().BeTrue();
    }

    private static async Task<Guid> SeedDeclarationAsync(ApplicationDbContext db)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Archive",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var fiscalYear = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            Year = 2025,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var declaration = new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            FiscalYearId = fiscalYear.Id,
            Year = 2025,
            Title = "Declaration Archive",
            Status = DeclarationStatus.Generated,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        db.GeneratedFiles.Add(new GeneratedFile
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            FileType = GeneratedFileType.FoundationDecemp,
            FileName = "foundation.txt",
            RelativePath = "clients/CLI01/2025/exports/foundation.txt",
            Sha256Hash = "hash",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        return declaration.Id;
    }

    private static ArchiveFixture CreateFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", "Archive", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
        var environment = new TestHostEnvironment();
        var storage = new DeclarationExportStorageService(
            Options.Create(new StorageOptions { RootPath = rootPath }),
            environment);
        var service = new ArchiveService(
            db,
            new FakeCurrentUserService(),
            environment,
            storage,
            new FileHashService());

        return new ArchiveFixture(db, service, rootPath);
    }

    private sealed record ArchiveFixture(ApplicationDbContext Db, ArchiveService Service, string RootPath) : IDisposable
    {
        public void Dispose()
        {
            Db.Dispose();
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;

        public string? UserName => null;

        public string? Role => null;

        public bool IsAuthenticated => false;
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "DeclarationEmployer.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
