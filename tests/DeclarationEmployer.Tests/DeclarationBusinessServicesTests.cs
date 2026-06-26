using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations.Validation;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class DeclarationBusinessServicesTests
{
    [Fact]
    public async Task BeneficiariesService_CreateAsync_CreatesBeneficiaryAndAudit()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var service = CreateBeneficiariesService(db);

        var result = await service.CreateAsync(declaration.Id, new CreateDeclarationBeneficiaryRequest
        {
            IdentifierType = "CIN",
            Identifier = "12345678",
            FullNameOrCompanyName = "Ali Test",
            Country = "Tunisie",
            IsResident = true
        });

        result.Identifier.Should().Be("12345678");
        db.DeclarationBeneficiaries.Should().ContainSingle(x => x.DeclarationId == declaration.Id);
        db.AuditLogs.Should().ContainSingle(x => x.Action == "BENEFICIARY_CREATED");
        db.DeclarationEvents.Should().ContainSingle(x => x.Action == "BENEFICIARY_CREATED");
    }

    [Fact]
    public async Task BeneficiariesService_DeleteAsync_RejectsWhenUsedByLine()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            IdentifierType = BeneficiaryIdentifierType.CIN,
            Identifier = "99887766",
            FullNameOrCompanyName = "Beneficiaire lie"
        };
        db.DeclarationBeneficiaries.Add(beneficiary);
        db.DeclarationLines.Add(new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            BeneficiaryId = beneficiary.Id,
            OperationType = "Honoraires",
            GrossAmount = 100m,
            TaxableAmount = 100m,
            Rate = 10m,
            WithheldAmount = 10m
        });
        await db.SaveChangesAsync();

        var service = CreateBeneficiariesService(db);
        var act = () => service.DeleteAsync(declaration.Id, beneficiary.Id);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task LinesService_CreateAsync_CreatesLineAndAudit()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            IdentifierType = BeneficiaryIdentifierType.CIN,
            Identifier = "11122233",
            FullNameOrCompanyName = "Beneficiaire Ligne"
        };
        db.DeclarationBeneficiaries.Add(beneficiary);
        await db.SaveChangesAsync();

        var service = CreateLinesService(db);
        var result = await service.CreateAsync(declaration.Id, new CreateDeclarationLineRequest
        {
            BeneficiaryId = beneficiary.Id,
            OperationType = "Honoraires",
            GrossAmount = 1200m,
            TaxableAmount = 1200m,
            Rate = 15m,
            WithheldAmount = 180m
        });

        result.OperationType.Should().Be("Honoraires");
        db.DeclarationLines.Should().ContainSingle(x => x.DeclarationId == declaration.Id);
        db.AuditLogs.Should().ContainSingle(x => x.Action == "LINE_CREATED");
        db.DeclarationEvents.Should().ContainSingle(x => x.Action == "LINE_CREATED");
    }

    [Fact]
    public async Task LinesService_UpdateAsync_RejectsWhenDeclarationLocked()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db, isLocked: true);
        var line = new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            OperationType = "Initial",
            GrossAmount = 100m,
            TaxableAmount = 100m,
            Rate = 10m,
            WithheldAmount = 10m
        };
        db.DeclarationLines.Add(line);
        await db.SaveChangesAsync();

        var service = CreateLinesService(db);
        var act = () => service.UpdateAsync(declaration.Id, line.Id, new UpdateDeclarationLineRequest
        {
            OperationType = "Modifie",
            GrossAmount = 150m,
            TaxableAmount = 150m,
            Rate = 10m,
            WithheldAmount = 15m,
            Status = DeclarationLineStatus.Draft.ToString()
        });

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task LinesService_DeleteAsync_CreatesAuditAndEvent()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var line = new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            OperationType = "A supprimer",
            GrossAmount = 100m,
            TaxableAmount = 100m,
            Rate = 10m,
            WithheldAmount = 10m
        };
        db.DeclarationLines.Add(line);
        await db.SaveChangesAsync();

        var service = CreateLinesService(db);
        await service.DeleteAsync(declaration.Id, line.Id);

        db.DeclarationLines.Should().BeEmpty();
        db.AuditLogs.Should().ContainSingle(x => x.Action == "LINE_DELETED");
        db.DeclarationEvents.Should().ContainSingle(x => x.Action == "LINE_DELETED");
    }

    [Fact]
    public async Task AnnexesService_CreateAsync_CreatesAnnex()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var service = CreateAnnexesService(db);

        var result = await service.CreateAsync(declaration.Id, new CreateDeclarationAnnexRequest
        {
            AnnexCode = "ANNEXE-1",
            Title = "Annexe principale"
        });

        result.AnnexCode.Should().Be("ANNEXE-1");
        db.DeclarationAnnexes.Should().ContainSingle(x => x.DeclarationId == declaration.Id);
        db.AuditLogs.Should().ContainSingle(x => x.Action == "ANNEX_CREATED");
    }

    [Fact]
    public async Task AnnexesService_DeleteAsync_RejectsWhenUsedByLine()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var annex = new DeclarationAnnex
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            AnnexCode = "ANN-USED",
            Title = "Annexe utilisee"
        };
        db.DeclarationAnnexes.Add(annex);
        db.DeclarationLines.Add(new DeclarationLine
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            AnnexId = annex.Id,
            OperationType = "Ligne annexe",
            GrossAmount = 200m,
            TaxableAmount = 200m,
            Rate = 10m,
            WithheldAmount = 20m
        });
        await db.SaveChangesAsync();

        var service = CreateAnnexesService(db);
        var act = () => service.DeleteAsync(declaration.Id, annex.Id);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task AnomaliesService_ResolveAsync_MarksResolvedAndWritesAudit()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        var anomaly = new DeclarationAnomaly
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            Severity = DeclarationAnomalySeverity.Blocking,
            Code = "DEC-001",
            Message = "Anomalie test"
        };
        db.DeclarationAnomalies.Add(anomaly);
        await db.SaveChangesAsync();

        var service = CreateAnomaliesService(db);
        var result = await service.ResolveAsync(declaration.Id, anomaly.Id, new ResolveDeclarationAnomalyRequest
        {
            Reason = "Controle effectue"
        });

        result.IsResolved.Should().BeTrue();
        db.DeclarationAnomalies.Single().ResolvedBy.Should().Be("system-dev");
        db.AuditLogs.Should().ContainSingle(x => x.Action == "ANOMALY_RESOLVED");
        db.DeclarationEvents.Should().ContainSingle(x => x.Action == "ANOMALY_RESOLVED");
    }

    [Fact]
    public async Task GeneratedFilesService_GetByDeclarationAsync_ReturnsFiles()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        db.GeneratedFiles.Add(new GeneratedFile
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            FileType = GeneratedFileType.ControlReport,
            FileName = "controle.txt",
            RelativePath = "generated/controle.txt"
        });
        await db.SaveChangesAsync();

        var service = new GeneratedFilesService(db);
        var result = await service.GetByDeclarationAsync(declaration.Id);

        result.Should().ContainSingle(x => x.FileName == "controle.txt");
    }

    [Fact]
    public async Task ArchivedDocumentsService_GetByClientAndYearAsync_FiltersCorrectly()
    {
        await using var db = CreateDbContext();
        var clientA = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI-A",
            RaisonSociale = "Client A",
            IsActive = true
        };
        var clientB = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI-B",
            RaisonSociale = "Client B",
            IsActive = true
        };
        var fyA = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = clientA.Id,
            Year = 2025
        };
        var fyB = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = clientB.Id,
            Year = 2024
        };
        var declarationA = new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = clientA.Id,
            FiscalYearId = fyA.Id,
            Year = 2025,
            Title = "Decl A"
        };
        var declarationB = new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = clientB.Id,
            FiscalYearId = fyB.Id,
            Year = 2024,
            Title = "Decl B"
        };
        db.Clients.AddRange(clientA, clientB);
        db.FiscalYears.AddRange(fyA, fyB);
        db.Declarations.AddRange(declarationA, declarationB);
        db.ArchivedDocuments.AddRange(
            new ArchivedDocument
            {
                Id = Guid.NewGuid(),
                DeclarationId = declarationA.Id,
                ClientCompanyId = clientA.Id,
                FiscalYearId = fyA.Id,
                DocumentType = ArchivedDocumentType.DeclarationExport,
                FileName = "decl-a.zip",
                RelativePath = "archive/decl-a.zip"
            },
            new ArchivedDocument
            {
                Id = Guid.NewGuid(),
                DeclarationId = declarationB.Id,
                ClientCompanyId = clientB.Id,
                FiscalYearId = fyB.Id,
                DocumentType = ArchivedDocumentType.SupportingDocument,
                FileName = "decl-b.zip",
                RelativePath = "archive/decl-b.zip"
            });
        await db.SaveChangesAsync();

        var service = new ArchivedDocumentsService(db);
        var result = await service.GetByClientAndYearAsync(clientA.Id, 2025);

        result.Should().ContainSingle(x => x.FileName == "decl-a.zip");
    }

    private static DeclarationAnnexesService CreateAnnexesService(ApplicationDbContext db)
    {
        return new DeclarationAnnexesService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateDeclarationAnnexRequestValidator(),
            new UpdateDeclarationAnnexRequestValidator());
    }

    private static DeclarationBeneficiariesService CreateBeneficiariesService(ApplicationDbContext db)
    {
        return new DeclarationBeneficiariesService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateDeclarationBeneficiaryRequestValidator(),
            new UpdateDeclarationBeneficiaryRequestValidator());
    }

    private static DeclarationLinesService CreateLinesService(ApplicationDbContext db)
    {
        return new DeclarationLinesService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateDeclarationLineRequestValidator(),
            new UpdateDeclarationLineRequestValidator());
    }

    private static DeclarationAnomaliesService CreateAnomaliesService(ApplicationDbContext db)
    {
        return new DeclarationAnomaliesService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new ResolveDeclarationAnomalyRequestValidator());
    }

    private static async Task<EmployerDeclaration> SeedEditableDeclarationAsync(
        ApplicationDbContext db,
        bool isLocked = false)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Test",
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
            Title = "Declaration test",
            Status = DeclarationStatus.Draft,
            IsLocked = isLocked,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        await db.SaveChangesAsync();

        return declaration;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
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
