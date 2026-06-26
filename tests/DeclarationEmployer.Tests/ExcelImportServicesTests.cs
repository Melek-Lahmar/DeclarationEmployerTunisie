using ClosedXML.Excel;
using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Import;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Import;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Tests;

public sealed class ExcelImportServicesTests
{
    [Fact]
    public async Task ExcelDeclarationImportService_Preview_ValidFile_ReturnsValidRows()
    {
        var stream = CreateWorkbookStream(
            [
                new Dictionary<string, object?>
                {
                    ["IdentifierType"] = "CIN",
                    ["Identifier"] = "12345678",
                    ["BeneficiaryName"] = "Ali Test",
                    ["OperationType"] = "Honoraires",
                    ["GrossAmount"] = 1000m,
                    ["TaxableAmount"] = 1000m,
                    ["Rate"] = 10m,
                    ["WithheldAmount"] = 100m
                }
            ]);

        var service = new ExcelDeclarationImportService();
        var result = await service.ParseAsync(stream);

        result.Rows.Should().ContainSingle();
        result.Rows.Single().IsValid.Should().BeTrue();
        result.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task ExcelDeclarationImportService_Preview_MissingRequiredColumn_ReturnsErrors()
    {
        var stream = CreateWorkbookStream(
            [
                new Dictionary<string, object?>
                {
                    ["IdentifierType"] = "CIN",
                    ["Identifier"] = "12345678",
                    ["BeneficiaryName"] = "Ali Test",
                    ["GrossAmount"] = 1000m,
                    ["TaxableAmount"] = 1000m,
                    ["Rate"] = 10m,
                    ["WithheldAmount"] = 100m
                }
            ]);

        var service = new ExcelDeclarationImportService();
        var result = await service.ParseAsync(stream);

        result.Issues.Should().Contain(x => x.Code == "MISSING_REQUIRED_COLUMN" && x.ColumnName == "OperationType");
    }

    [Fact]
    public async Task ExcelDeclarationImportService_Preview_InvalidAmount_ReturnsErrors()
    {
        var stream = CreateWorkbookStream(
            [
                new Dictionary<string, object?>
                {
                    ["IdentifierType"] = "CIN",
                    ["Identifier"] = "12345678",
                    ["BeneficiaryName"] = "Ali Test",
                    ["OperationType"] = "Honoraires",
                    ["GrossAmount"] = -10m,
                    ["TaxableAmount"] = 1000m,
                    ["Rate"] = 10m,
                    ["WithheldAmount"] = 100m
                }
            ]);

        var service = new ExcelDeclarationImportService();
        var result = await service.ParseAsync(stream);

        result.Rows.Should().ContainSingle();
        result.Rows.Single().IsValid.Should().BeFalse();
        result.Issues.Should().Contain(x => x.ColumnName == "GrossAmount");
    }

    [Fact]
    public async Task DeclarationImportService_Commit_ImportsValidRows()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        using var fixture = CreateImportFixture(db);

        var preview = await fixture.Service.PreviewAsync(
            declaration.Id,
            CreateWorkbookStream(
                [
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "12345678",
                        ["BeneficiaryName"] = "Ali Test",
                        ["OperationType"] = "Honoraires",
                        ["GrossAmount"] = 1000m,
                        ["TaxableAmount"] = 1000m,
                        ["Rate"] = 10m,
                        ["WithheldAmount"] = 100m
                    }
                ]),
            "import.xlsx");

        var result = await fixture.Service.CommitAsync(declaration.Id, new ExcelImportCommitRequest
        {
            TemporaryFileToken = preview.TemporaryFileToken,
            ImportOnlyValidRows = true
        });

        result.ImportedRows.Should().Be(1);
        db.DeclarationLines.Should().ContainSingle();
        db.Declarations.Single().Status.Should().Be(DeclarationStatus.Imported);
    }

    [Fact]
    public async Task DeclarationImportService_Commit_CreatesBeneficiaries()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        using var fixture = CreateImportFixture(db);

        var preview = await fixture.Service.PreviewAsync(
            declaration.Id,
            CreateWorkbookStream(
                [
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "87654321",
                        ["BeneficiaryName"] = "Sarra Test",
                        ["OperationType"] = "Salaires",
                        ["GrossAmount"] = 2000m,
                        ["TaxableAmount"] = 1800m,
                        ["Rate"] = 15m,
                        ["WithheldAmount"] = 270m
                    }
                ]),
            "beneficiary.xlsx");

        var result = await fixture.Service.CommitAsync(declaration.Id, new ExcelImportCommitRequest
        {
            TemporaryFileToken = preview.TemporaryFileToken,
            ImportOnlyValidRows = true
        });

        result.CreatedBeneficiaries.Should().Be(1);
        db.DeclarationBeneficiaries.Should().ContainSingle(x => x.Identifier == "87654321");
    }

    [Fact]
    public async Task DeclarationImportService_Commit_RejectsLockedDeclaration()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db, isLocked: true);
        using var fixture = CreateImportFixture(db);

        var token = await fixture.Storage.SaveTemporaryImportFileAsync(
            CreateWorkbookStream(
                [
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "12345678",
                        ["BeneficiaryName"] = "Ali Test",
                        ["OperationType"] = "Honoraires",
                        ["GrossAmount"] = 1000m,
                        ["TaxableAmount"] = 1000m,
                        ["Rate"] = 10m,
                        ["WithheldAmount"] = 100m
                    }
                ]),
            "locked.xlsx");

        var act = () => fixture.Service.CommitAsync(declaration.Id, new ExcelImportCommitRequest
        {
            TemporaryFileToken = token,
            ImportOnlyValidRows = true
        });

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task DeclarationImportService_Commit_WritesAuditAndEvent()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        using var fixture = CreateImportFixture(db);

        var preview = await fixture.Service.PreviewAsync(
            declaration.Id,
            CreateWorkbookStream(
                [
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "12345678",
                        ["BeneficiaryName"] = "Ali Test",
                        ["OperationType"] = "Honoraires",
                        ["GrossAmount"] = 1000m,
                        ["TaxableAmount"] = 1000m,
                        ["Rate"] = 10m,
                        ["WithheldAmount"] = 100m
                    }
                ]),
            "audit.xlsx");

        await fixture.Service.CommitAsync(declaration.Id, new ExcelImportCommitRequest
        {
            TemporaryFileToken = preview.TemporaryFileToken,
            ImportOnlyValidRows = true
        });

        db.AuditLogs.Should().Contain(x => x.Action == "IMPORT_PREVIEWED");
        db.AuditLogs.Should().Contain(x => x.Action == "IMPORT_COMPLETED");
        db.DeclarationEvents.Should().Contain(x => x.Action == "IMPORT_PREVIEWED");
        db.DeclarationEvents.Should().Contain(x => x.Action == "IMPORT_COMPLETED");
    }

    [Fact]
    public async Task DeclarationImportService_Commit_WithInvalidRows_ImportsOnlyValidRows()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedEditableDeclarationAsync(db);
        using var fixture = CreateImportFixture(db);

        var preview = await fixture.Service.PreviewAsync(
            declaration.Id,
            CreateWorkbookStream(
                [
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "12345678",
                        ["BeneficiaryName"] = "Ali Test",
                        ["OperationType"] = "Honoraires",
                        ["GrossAmount"] = 1000m,
                        ["TaxableAmount"] = 1000m,
                        ["Rate"] = 10m,
                        ["WithheldAmount"] = 100m
                    },
                    new Dictionary<string, object?>
                    {
                        ["IdentifierType"] = "CIN",
                        ["Identifier"] = "99999999",
                        ["BeneficiaryName"] = "Bad Row",
                        ["OperationType"] = "Honoraires",
                        ["GrossAmount"] = -5m,
                        ["TaxableAmount"] = 100m,
                        ["Rate"] = 10m,
                        ["WithheldAmount"] = 10m
                    }
                ]),
            "mixed.xlsx");

        var result = await fixture.Service.CommitAsync(declaration.Id, new ExcelImportCommitRequest
        {
            TemporaryFileToken = preview.TemporaryFileToken,
            ImportOnlyValidRows = true
        });

        result.ImportedRows.Should().Be(1);
        result.SkippedRows.Should().Be(1);
        result.CreatedAnomalies.Should().Be(1);
        db.DeclarationLines.Should().ContainSingle();
        db.DeclarationAnomalies.Should().ContainSingle(x => x.Code == "EXCEL_IMPORT_INVALID_ROW");
    }

    private static MemoryStream CreateWorkbookStream(IReadOnlyList<Dictionary<string, object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Import");

        var headers = rows.Count == 0
            ? new[]
            {
                "IdentifierType", "Identifier", "BeneficiaryName", "OperationType",
                "GrossAmount", "TaxableAmount", "Rate", "WithheldAmount"
            }.ToList()
            : rows.SelectMany(x => x.Keys).Distinct().ToList();

        for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
        {
            worksheet.Cell(1, columnIndex + 1).Value = headers[columnIndex];
        }

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            for (var columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                row.TryGetValue(headers[columnIndex], out var value);
                worksheet.Cell(rowIndex + 2, columnIndex + 1).Value = value?.ToString() ?? string.Empty;
            }
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<EmployerDeclaration> SeedEditableDeclarationAsync(
        ApplicationDbContext db,
        bool isLocked = false)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Import",
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
            Title = "Declaration Import",
            IsLocked = isLocked,
            Status = DeclarationStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        await db.SaveChangesAsync();

        return declaration;
    }

    private static ImportFixture CreateImportFixture(ApplicationDbContext db)
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        var environment = new TestHostEnvironment();
        var currentUser = new FakeCurrentUserService();
        var parser = new ExcelDeclarationImportService();
        var storage = new TemporaryFileStorageService(
            Options.Create(new StorageOptions { RootPath = rootPath }),
            environment);
        var service = new DeclarationImportService(db, currentUser, environment, parser, storage);

        return new ImportFixture(service, storage, rootPath);
    }

    private sealed record ImportFixture(
        DeclarationImportService Service,
        ITemporaryFileStorageService Storage,
        string RootPath) : IDisposable
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
