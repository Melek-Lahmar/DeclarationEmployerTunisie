using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Contracts.Generation;
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

public sealed class DeclarationExportServicesTests
{
    [Fact]
    public async Task FileHashService_SameContent_ReturnsSameHash()
    {
        using var fixture = CreateTempDirectory();
        var path = Path.Combine(fixture.RootPath, "same.txt");
        await File.WriteAllTextAsync(path, "same-content");
        var service = new FileHashService();

        var first = await service.ComputeSha256Async(path);
        var second = await service.ComputeSha256Async(path);

        first.Should().Be(second);
        first.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task FileHashService_DifferentContent_ReturnsDifferentHash()
    {
        using var fixture = CreateTempDirectory();
        var firstPath = Path.Combine(fixture.RootPath, "a.txt");
        var secondPath = Path.Combine(fixture.RootPath, "b.txt");
        await File.WriteAllTextAsync(firstPath, "content-a");
        await File.WriteAllTextAsync(secondPath, "content-b");
        var service = new FileHashService();

        var first = await service.ComputeSha256Async(firstPath);
        var second = await service.ComputeSha256Async(secondPath);

        first.Should().NotBe(second);
    }

    [Fact]
    public void InternalDeclarationCsvGenerator_GeneratesHeaderAndLines()
    {
        var generator = new InternalDeclarationCsvGenerator();
        var content = generator.Generate(new InternalDeclarationCsvDocument
        {
            DeclarationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ClientCode = "CLI01",
            ClientName = "Societe Export",
            FiscalYear = 2025,
            Lines =
            [
                new InternalDeclarationCsvLine
                {
                    LineId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    BeneficiaryIdentifierType = BeneficiaryIdentifierType.CIN,
                    BeneficiaryIdentifier = "12345678",
                    BeneficiaryName = "Ali Export",
                    OperationType = "Honoraires",
                    FiscalCategory = "BNC",
                    GrossAmount = 1000m,
                    TaxableAmount = 900m,
                    Rate = 10m,
                    WithheldAmount = 90m,
                    PaymentDate = new DateTime(2025, 3, 31),
                    DocumentReference = "DOC-EXP-001",
                    Notes = "Ligne test",
                    Status = DeclarationLineStatus.Controlled
                }
            ]
        });

        content.Should().Contain("DeclarationId;ClientCode;ClientName;FiscalYear;LineId");
        content.Should().Contain("\"CLI01\"");
        content.Should().Contain("\"Ali Export\"");
        content.Should().Contain("\"2025-03-31\"");
    }

    [Fact]
    public async Task DeclarationExportService_Preview_WithBlockingAnomalies_CannotGenerate()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        await SeedAnomalyAsync(db, declaration.Id, DeclarationAnomalySeverity.Blocking, "BLOQ-001", "Blocage export");
        using var fixture = CreateExportFixture(db);

        var result = await fixture.Service.PreviewAsync(declaration.Id);

        result.CanGenerate.Should().BeFalse();
        result.BlockingAnomaliesCount.Should().Be(1);
        result.BlockingMessages.Should().Contain("Blocage export");
    }

    [Fact]
    public async Task DeclarationExportService_Preview_WithValidControlledDeclaration_CanGenerate()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        var result = await fixture.Service.PreviewAsync(declaration.Id);

        result.CanGenerate.Should().BeTrue();
        result.LinesCount.Should().Be(1);
        result.TotalGrossAmount.Should().Be(1000m);
    }

    [Fact]
    public async Task DeclarationExportService_Generate_RejectsNoLines()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        using var fixture = CreateExportFixture(db);

        var act = () => fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        await act.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage("Impossible de generer un export sans lignes de declaration.");
    }

    [Fact]
    public async Task DeclarationExportService_Generate_RejectsBlockingAnomalies()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        await SeedAnomalyAsync(db, declaration.Id, DeclarationAnomalySeverity.Blocking, "BLOQ-001", "Blocage export");
        using var fixture = CreateExportFixture(db);

        var act = () => fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        await act.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage("Des anomalies bloquantes non resolues empechent la generation.");
    }

    [Fact]
    public async Task DeclarationExportService_Generate_CreatesFileAndGeneratedFile()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        var result = await fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        result.FileName.Should().EndWith(".csv");
        result.RelativePath.Should().Contain("clients/");
        result.Sha256Hash.Should().NotBeNullOrWhiteSpace();
        db.GeneratedFiles.Should().ContainSingle(x => x.DeclarationId == declaration.Id && x.FileType == GeneratedFileType.InternalExport);
        File.Exists(Path.Combine(fixture.RootPath, result.RelativePath.Replace('/', Path.DirectorySeparatorChar))).Should().BeTrue();
    }

    [Fact]
    public async Task DeclarationExportService_Generate_SetsStatusGenerated()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        var result = await fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        result.DeclarationStatus.Should().Be(DeclarationStatus.Generated.ToString());
        db.Declarations.Single().Status.Should().Be(DeclarationStatus.Generated);
    }

    [Fact]
    public async Task DeclarationExportService_Generate_WritesAuditAndEvent()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        await fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV", Notes = "Test export" });

        db.AuditLogs.Should().Contain(x => x.Action == "INTERNAL_EXPORT_GENERATED");
        db.DeclarationEvents.Should().Contain(x => x.Action == "INTERNAL_EXPORT_GENERATED");
    }

    [Fact]
    public async Task DeclarationExportService_Generate_RejectsClosedDeclaration()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Closed);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        var act = () => fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        await act.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage("Impossible de generer un export pour une declaration cloturee ou archivee.");
    }

    [Fact]
    public async Task DeclarationExportService_Generate_RejectsArchivedDeclaration()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Archived);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateExportFixture(db);

        var act = () => fixture.Service.GenerateAsync(declaration.Id, new GenerateDeclarationExportRequest { Format = "CSV" });

        await act.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage("Impossible de generer un export pour une declaration cloturee ou archivee.");
    }

    [Fact]
    public async Task GenerationService_Generate_RejectsOfficialMode()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateGenerationFixture(db);

        var act = () => fixture.Service.GenerateAsync(
            declaration.Id,
            new GenerateDeclarationFilesRequest { OfficialModeRequested = true });

        await act.Should().ThrowAsync<ApplicationConflictException>()
            .WithMessage(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
    }

    [Fact]
    public async Task GenerationService_Generate_CreatesFoundationFiles()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Controlled);
        await SeedLineAsync(db, declaration.Id);
        using var fixture = CreateGenerationFixture(db);

        var result = await fixture.Service.GenerateAsync(
            declaration.Id,
            new GenerateDeclarationFilesRequest());

        result.IsOfficialMode.Should().BeFalse();
        result.Message.Should().Be(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
        result.Files.Should().HaveCount(2);
        result.Files.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Sha256Hash));
        db.GeneratedFiles.Should().Contain(x => x.FileType == GeneratedFileType.FoundationDecemp);
        db.GeneratedFiles.Should().Contain(x => x.FileType == GeneratedFileType.FoundationAnnex);
        result.Files.Should().OnlyContain(x => File.Exists(Path.Combine(fixture.RootPath, x.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<EmployerDeclaration> SeedDeclarationAsync(ApplicationDbContext db, DeclarationStatus status)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Export",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var fiscalYear = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            Year = 2025,
            IsClosed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        var declaration = new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            FiscalYearId = fiscalYear.Id,
            Year = 2025,
            Title = "Declaration Export",
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        await db.SaveChangesAsync();
        return declaration;
    }

    private static async Task SeedLineAsync(ApplicationDbContext db, Guid declarationId)
    {
        var beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = BeneficiaryIdentifierType.CIN,
            Identifier = "12345678",
            FullNameOrCompanyName = "Ali Export",
            IsResident = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.DeclarationBeneficiaries.Add(beneficiary);
        db.DeclarationLines.Add(new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            BeneficiaryId = beneficiary.Id,
            OperationType = "Honoraires",
            FiscalCategory = "BNC",
            GrossAmount = 1000m,
            TaxableAmount = 900m,
            Rate = 10m,
            WithheldAmount = 90m,
            PaymentDate = new DateTime(2025, 3, 31),
            DocumentReference = "DOC-EXP-001",
            Notes = "Ligne export test",
            Status = DeclarationLineStatus.Controlled,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedAnomalyAsync(
        ApplicationDbContext db,
        Guid declarationId,
        DeclarationAnomalySeverity severity,
        string code,
        string message)
    {
        db.DeclarationAnomalies.Add(new DeclarationAnomaly
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            Severity = severity,
            Code = code,
            Message = message,
            IsResolved = false,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private static ExportFixture CreateExportFixture(ApplicationDbContext db)
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", "Exports", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        var environment = new TestHostEnvironment();
        var currentUser = new FakeCurrentUserService();
        var storage = new DeclarationExportStorageService(
            Options.Create(new StorageOptions { RootPath = rootPath }),
            environment);
        var generatedFilesService = new GeneratedFilesService(db);
        var service = new DeclarationExportService(
            db,
            currentUser,
            environment,
            storage,
            new FileHashService(),
            new InternalDeclarationCsvGenerator(),
            generatedFilesService);

        return new ExportFixture(service, rootPath);
    }

    private static GenerationFixture CreateGenerationFixture(ApplicationDbContext db)
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", "Generation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        var environment = new TestHostEnvironment();
        var storage = new DeclarationExportStorageService(
            Options.Create(new StorageOptions { RootPath = rootPath }),
            environment);
        var service = new GenerationService(
            db,
            new FakeCurrentUserService(),
            environment,
            storage,
            new FileHashService());

        return new GenerationFixture(service, rootPath);
    }

    private static TempDirectoryFixture CreateTempDirectory()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", "Hash", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
        return new TempDirectoryFixture(rootPath);
    }

    private sealed record ExportFixture(DeclarationExportService Service, string RootPath) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
    }

    private sealed record GenerationFixture(GenerationService Service, string RootPath) : IDisposable
    {
        public void Dispose()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
    }

    private sealed record TempDirectoryFixture(string RootPath) : IDisposable
    {
        public void Dispose()
        {
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
