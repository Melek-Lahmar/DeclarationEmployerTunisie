using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Declarations.Validation;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaPriorityAnnexServiceTests
{
    [Fact]
    public async Task CreateA1LineAsync_PersistsDetailedFieldsAndSummary()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = CreateService(db);
        await service.CreateA1LineAsync(declarationId, new CreateEmpccaAnnexA1LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(2),
            FamilySituation = 2,
            DependentChildrenCount = 1,
            WorkPeriodStart = new DateOnly(2025, 1, 1),
            WorkPeriodEnd = new DateOnly(2025, 12, 31),
            WorkPeriodDays = 365,
            TaxableIncome = 1000,
            BenefitsInKind = 50,
            GrossTaxableIncome = 1050,
            CommonRegimeWithheldAmount = 100,
            SocialSolidarityContribution = 5,
            NetPaidAmount = 945
        });

        var summary = await service.GetA1SummaryAsync(declarationId);
        summary.LineCount.Should().Be(1);
        summary.GrossTaxableIncomeTotal.Should().Be(1050);
        summary.CommonRegimeWithheldTotal.Should().Be(100);
        db.AnnexA1Details.Should().ContainSingle(x => x.WorkPeriodDays == 365);
    }

    [Fact]
    public async Task CreateA2LineAsync_RejectsDuplicateOrderNumber()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = CreateService(db);
        var request = new CreateEmpccaAnnexA2LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            AmountType = 1,
            GrossProfessionalAmount = 1000,
            WithheldAmount = 150,
            NetPaidAmount = 850
        };
        await service.CreateA2LineAsync(declarationId, request);

        var action = () => service.CreateA2LineAsync(declarationId, request);
        await action.Should().ThrowAsync<Exception>().WithMessage("*numero d'ordre 1*");
    }

    [Fact]
    public async Task CreateA5LineAsync_PersistsNewThreePercentField()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = CreateService(db);
        await service.CreateA5LineAsync(declarationId, new CreateEmpccaAnnexA5LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            PurchasesFromFifteenPercentCompanies = 2000,
            DeliveryPlatformThreePercentWithheldAmount = 60,
            WithheldAmount = 60,
            NetPaidAmount = 1940
        });

        var summary = await service.GetA5SummaryAsync(declarationId);
        summary.DeliveryPlatformThreePercentWithheldTotal.Should().Be(60);
        db.AnnexA5Details.Should().ContainSingle(x => x.DeliveryPlatformThreePercentWithheldAmount == 60);
    }

    private static EmpccaPriorityAnnexService CreateService(ApplicationDbContext db) => new(
        db, new FakeCurrentUserService(), new TestHostEnvironment(),
        new CreateEmpccaAnnexA1LineRequestValidator(),
        new CreateEmpccaAnnexA2LineRequestValidator(),
        new CreateEmpccaAnnexA5LineRequestValidator());

    private static EmpccaBeneficiaryInput Beneficiary(int type) => new()
    {
        IdentifierType = type,
        Identifier = type == 2 ? "12345678" : "1234567A/B000",
        Name = "Beneficiaire Test",
        Activity = "Conseil",
        JobTitle = "Consultant",
        Address = "10 rue de Tunis"
    };

    private static async Task<Guid> SeedDeclarationAsync(ApplicationDbContext db)
    {
        var client = new ClientCompany { Code = "CLI01", RaisonSociale = "Societe Test" };
        var year = new FiscalYear { ClientCompanyId = client.Id, Year = 2025 };
        var declaration = new EmployerDeclaration
        {
            ClientCompanyId = client.Id, FiscalYearId = year.Id, Year = 2025,
            Title = "Declaration 2025"
        };
        db.AddRange(client, year, declaration);
        await db.SaveChangesAsync();
        return declaration.Id;
    }

    private static ApplicationDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

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
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
