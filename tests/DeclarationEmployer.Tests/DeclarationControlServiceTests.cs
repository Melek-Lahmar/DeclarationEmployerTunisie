using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations.Validation;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.FiscalEngine;
using DeclarationEmployer.FiscalEngine.Rules;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class DeclarationControlServiceTests
{
    [Fact]
    public async Task DeclarationControlService_ControlAsync_CreatesAnomalies()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Imported);
        await SeedLineAsync(db, declaration.Id, grossAmount: -1m);
        var service = CreateControlService(db);

        var result = await service.ControlAsync(declaration.Id);

        result.BlockingAnomaliesCount.Should().BeGreaterThan(0);
        db.DeclarationAnomalies.Should().Contain(x => x.Code == "LINE_GROSS_AMOUNT_NEGATIVE");
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_ValidLines_SetsStatusControlled()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Imported);
        await SeedLineAsync(db, declaration.Id);
        var service = CreateControlService(db);

        var result = await service.ControlAsync(declaration.Id);

        result.DeclarationStatus.Should().Be(DeclarationStatus.Controlled.ToString());
        db.Declarations.Single().Status.Should().Be(DeclarationStatus.Controlled);
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_BlockingIssues_DoesNotSetControlled()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Imported);
        await SeedLineAsync(db, declaration.Id, rate: 120m);
        var service = CreateControlService(db);

        var result = await service.ControlAsync(declaration.Id);

        result.DeclarationStatus.Should().Be(DeclarationStatus.Imported.ToString());
        db.Declarations.Single().Status.Should().Be(DeclarationStatus.Imported);
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_WritesAuditAndEvent()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Imported);
        await SeedLineAsync(db, declaration.Id);
        var service = CreateControlService(db);

        await service.ControlAsync(declaration.Id);

        db.AuditLogs.Should().Contain(x => x.Action == "CONTROL_EXECUTED");
        db.DeclarationEvents.Should().Contain(x => x.Action == "DECLARATION_CONTROLLED");
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_ReplacesPreviousUnresolvedAnomalies()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Imported);
        await SeedLineAsync(db, declaration.Id, grossAmount: -1m);
        db.DeclarationAnomalies.Add(new DeclarationAnomaly
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            Severity = DeclarationAnomalySeverity.Blocking,
            Code = "LINE_GROSS_AMOUNT_NEGATIVE",
            Message = "Ancienne anomalie",
            IsResolved = false,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        db.DeclarationAnomalies.Add(new DeclarationAnomaly
        {
            Id = Guid.NewGuid(),
            DeclarationId = declaration.Id,
            Severity = DeclarationAnomalySeverity.Warning,
            Code = "LINE_RATE_ZERO",
            Message = "Anomalie resolue",
            IsResolved = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var service = CreateControlService(db);
        await service.ControlAsync(declaration.Id);

        db.DeclarationAnomalies.Count(x => x.Code == "LINE_GROSS_AMOUNT_NEGATIVE" && !x.IsResolved).Should().Be(1);
        db.DeclarationAnomalies.Count(x => x.Code == "LINE_RATE_ZERO" && x.IsResolved).Should().Be(1);
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_RejectsClosedDeclaration()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Closed);
        var service = CreateControlService(db);

        var act = () => service.ControlAsync(declaration.Id);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task DeclarationControlService_ControlAsync_RejectsArchivedDeclaration()
    {
        await using var db = CreateDbContext();
        var declaration = await SeedDeclarationAsync(db, DeclarationStatus.Archived);
        var service = CreateControlService(db);

        var act = () => service.ControlAsync(declaration.Id);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    private static DeclarationControlService CreateControlService(ApplicationDbContext db)
    {
        var anomaliesService = new DeclarationAnomaliesService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new ResolveDeclarationAnomalyRequestValidator());

        return new DeclarationControlService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            CreateEngine(),
            anomaliesService);
    }

    private static IFiscalControlEngine CreateEngine()
    {
        return new FiscalControlEngine(
        [
            new GrossAmountMustBePositiveOrZeroRule(),
            new TaxableAmountMustBePositiveOrZeroRule(),
            new WithheldAmountMustBePositiveOrZeroRule(),
            new RateMustBeBetweenZeroAndHundredRule(),
            new WithheldAmountMustNotExceedTaxableAmountRule(),
            new BeneficiaryRequiredRule(),
            new OperationTypeRequiredRule(),
            new PaymentDateMustBeInsideFiscalYearRule(),
            new ZeroRateWarningRule(),
            new MissingDocumentReferenceWarningRule(),
            new ZeroTaxableWithWithheldAmountWarningRule(),
            new MissingFiscalCategoryInfoRule()
        ]);
    }

    private static async Task<EmployerDeclaration> SeedDeclarationAsync(
        ApplicationDbContext db,
        DeclarationStatus status)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Controle",
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
            Title = "Declaration Controle",
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        await db.SaveChangesAsync();

        return declaration;
    }

    private static async Task SeedLineAsync(
        ApplicationDbContext db,
        Guid declarationId,
        decimal grossAmount = 100m,
        decimal taxableAmount = 100m,
        decimal rate = 10m,
        decimal withheldAmount = 10m)
    {
        var beneficiary = new DeclarationBeneficiary
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            IdentifierType = BeneficiaryIdentifierType.CIN,
            Identifier = "12345678",
            FullNameOrCompanyName = "Ali Controle",
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
            GrossAmount = grossAmount,
            TaxableAmount = taxableAmount,
            Rate = rate,
            WithheldAmount = withheldAmount,
            PaymentDate = new DateTime(2025, 3, 31),
            DocumentReference = "DOC-CTRL-001",
            Status = DeclarationLineStatus.Imported,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
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
