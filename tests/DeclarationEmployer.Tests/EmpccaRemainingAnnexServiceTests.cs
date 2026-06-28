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

public sealed class EmpccaRemainingAnnexServiceTests
{
    [Fact]
    public async Task UpdateA7LineAsync_ShouldPersistModifiedValues()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = CreateService(db);
        var created = await service.CreateA7LineAsync(declarationId, new CreateEmpccaAnnexA7LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            PaidAmountType = 29,
            PaidAmount = 1000,
            WithheldAmount = 10,
            NetPaidAmount = 990
        });

        var updated = await service.UpdateA7LineAsync(declarationId, created.Id, new CreateEmpccaAnnexA7LineRequest
        {
            OrderNumber = 1,
            Beneficiary = Beneficiary(1),
            PaidAmountType = 29,
            PaidAmount = 1200,
            WithheldAmount = 12,
            NetPaidAmount = 1188
        });

        updated.Details.PaidAmount.Should().Be(1200);
        db.AnnexA7Details.Should().ContainSingle(x => x.PaidAmount == 1200 && x.WithheldAmount == 12);
    }

    private static EmpccaRemainingAnnexService CreateService(ApplicationDbContext db) => new(
        db, new FakeCurrentUserService(), new TestHostEnvironment(),
        new CreateEmpccaAnnexA3LineRequestValidator(),
        new CreateEmpccaAnnexA4LineRequestValidator(),
        new CreateEmpccaAnnexA6LineRequestValidator(),
        new CreateEmpccaAnnexA7LineRequestValidator());

    private static EmpccaBeneficiaryInput Beneficiary(int type) => new()
    {
        IdentifierType = type,
        Identifier = "1234567A/B000",
        Name = "BENEFICIARY",
        Address = "TUNIS"
    };

    private static async Task<Guid> SeedDeclarationAsync(ApplicationDbContext db)
    {
        var client = new ClientCompany { Code = "CLI02", RaisonSociale = "Societe Test 2" };
        var year = new FiscalYear { ClientCompanyId = client.Id, Year = 2025 };
        var declaration = new EmployerDeclaration
        {
            ClientCompanyId = client.Id,
            FiscalYearId = year.Id,
            Year = 2025,
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
