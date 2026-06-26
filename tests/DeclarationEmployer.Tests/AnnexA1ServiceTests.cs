using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Declarations.AnnexA1;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class AnnexA1ServiceTests
{
    [Fact]
    public async Task CreateLineAsync_CreatesAnnexBeneficiaryAndLine()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        var line = await service.CreateLineAsync(declarationId, new CreateAnnexA1LineRequest
        {
            BeneficiaryIdentifierType = "CIN",
            BeneficiaryIdentifier = "12345678",
            BeneficiaryName = "Employe Test",
            GrossAmount = 1000,
            TaxableAmount = 900,
            Rate = 10,
            WithheldAmount = 90,
            PaymentDate = new DateTime(2025, 1, 31)
        });

        line.BeneficiaryName.Should().Be("Employe Test");
        line.NetPaidAmount.Should().Be(910);
        db.DeclarationAnnexes.Should().ContainSingle(x => x.DeclarationId == declarationId && x.AnnexCode == "A1");
        db.DeclarationBeneficiaries.Should().ContainSingle(x => x.Identifier == "12345678");
        db.DeclarationLines.Should().ContainSingle(x => x.Id == line.Id);
        db.AuditLogs.Should().Contain(x => x.Action == "ANNEX_A1_LINE_CREATED");
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsTotalsAndUnconfirmedMappingMessage()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        await service.CreateLineAsync(declarationId, new CreateAnnexA1LineRequest
        {
            BeneficiaryIdentifier = "MF001",
            BeneficiaryName = "Employe Test",
            GrossAmount = 1000,
            TaxableAmount = 900,
            Rate = 10,
            WithheldAmount = 90
        });

        var summary = await service.GetSummaryAsync(declarationId);

        summary.LinesCount.Should().Be(1);
        summary.BeneficiariesCount.Should().Be(1);
        summary.GrossAmountTotal.Should().Be(1000);
        summary.WithheldAmountTotal.Should().Be(90);
        summary.IsOfficialMappingConfirmed.Should().BeFalse();
        summary.MappingMessage.Should().Be(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
    }

    [Fact]
    public async Task ValidateLineAsync_ReturnsWarningsForMissingPaymentDate()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        var line = await service.CreateLineAsync(declarationId, new CreateAnnexA1LineRequest
        {
            BeneficiaryIdentifier = "MF001",
            BeneficiaryName = "Employe Test",
            GrossAmount = 1000,
            TaxableAmount = 900,
            Rate = 10,
            WithheldAmount = 90
        });

        var validation = await service.ValidateLineAsync(declarationId, line.Id);

        validation.IsValid.Should().BeTrue();
        validation.Warnings.Should().ContainSingle(x => x == "Date de paiement absente.");
    }

    [Fact]
    public async Task CreateLineAsync_RejectsClosedDeclaration()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db, DeclarationStatus.Closed, isLocked: true);
        var service = CreateService(db);

        var act = () => service.CreateLineAsync(declarationId, new CreateAnnexA1LineRequest
        {
            BeneficiaryIdentifier = "MF001",
            BeneficiaryName = "Employe Test",
            GrossAmount = 1000,
            TaxableAmount = 900,
            Rate = 10,
            WithheldAmount = 90
        });

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    private static AnnexA1Service CreateService(ApplicationDbContext db)
    {
        return new AnnexA1Service(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment());
    }

    private static async Task<Guid> SeedOpenDeclarationAsync(
        ApplicationDbContext db,
        DeclarationStatus status = DeclarationStatus.Draft,
        bool isLocked = false)
    {
        var clientId = Guid.NewGuid();
        var fiscalYearId = Guid.NewGuid();
        var declarationId = Guid.NewGuid();

        db.Clients.Add(new ClientCompany
        {
            Id = clientId,
            Code = "CLI01",
            RaisonSociale = "Societe Test",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            ClientCompanyId = clientId,
            Year = 2025,
            IsClosed = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.Declarations.Add(new EmployerDeclaration
        {
            Id = declarationId,
            ClientCompanyId = clientId,
            FiscalYearId = fiscalYearId,
            Year = 2025,
            Status = status,
            Title = "Declaration employeur 2025",
            IsLocked = isLocked,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        return declarationId;
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
        public Guid? UserId => Guid.NewGuid();

        public string? UserName => "tester";

        public string? Role => UserRole.Admin.ToString();

        public bool IsAuthenticated => true;
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
