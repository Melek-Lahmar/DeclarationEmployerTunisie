using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Declarations.AnnexFoundation;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class AnnexFoundationServiceTests
{
    [Fact]
    public async Task CreateLineAsync_CreatesFoundationLineForAllowedAnnex()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        var line = await service.CreateLineAsync(declarationId, "a2", new CreateAnnexFoundationLineRequest
        {
            OrderNumber = 1,
            BeneficiaryIdentifier = "BEN001",
            BeneficiaryName = "Beneficiaire Test",
            OperationType = "Honoraires",
            GrossAmount = 1000,
            WithholdingAmount = 150,
            NetAmount = 850
        });

        line.AnnexCode.Should().Be("A2");
        line.BeneficiaryName.Should().Be("Beneficiaire Test");
        line.IsOfficialMappingConfirmed.Should().BeFalse();
        db.DeclarationAnnexes.Should().ContainSingle(x => x.DeclarationId == declarationId && x.AnnexCode == "A2");
        db.DeclarationLines.Should().ContainSingle(x => x.Id == line.Id);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsFoundationTotalsAndMappingMessage()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        await service.CreateLineAsync(declarationId, "A7", new CreateAnnexFoundationLineRequest
        {
            BeneficiaryIdentifier = "BEN001",
            BeneficiaryName = "Beneficiaire Test",
            OperationType = "Operation foundation",
            GrossAmount = 1000,
            WithholdingAmount = 100,
            NetAmount = 900
        });

        var summary = await service.GetSummaryAsync(declarationId, "A7");

        summary.AnnexCode.Should().Be("A7");
        summary.LinesCount.Should().Be(1);
        summary.GrossAmountTotal.Should().Be(1000);
        summary.WithholdingAmountTotal.Should().Be(100);
        summary.NetAmountTotal.Should().Be(900);
        summary.IsOfficialMappingConfirmed.Should().BeFalse();
        summary.MappingMessage.Should().Be(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
    }

    [Fact]
    public async Task CreateLineAsync_RejectsInvalidAnnexCode()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedOpenDeclarationAsync(db);
        var service = CreateService(db);

        var act = () => service.CreateLineAsync(declarationId, "A9", new CreateAnnexFoundationLineRequest
        {
            BeneficiaryIdentifier = "BEN001",
            BeneficiaryName = "Beneficiaire Test",
            OperationType = "Operation foundation",
            GrossAmount = 1000,
            WithholdingAmount = 100,
            NetAmount = 900
        });

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    private static AnnexFoundationService CreateService(ApplicationDbContext db)
    {
        return new AnnexFoundationService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment());
    }

    private static async Task<Guid> SeedOpenDeclarationAsync(ApplicationDbContext db)
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
            Status = DeclarationStatus.Draft,
            Title = "Declaration employeur 2025",
            IsLocked = false,
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
