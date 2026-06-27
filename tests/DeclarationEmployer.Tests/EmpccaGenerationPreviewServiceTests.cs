using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Domain.Declarations.Empcca;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class EmpccaGenerationPreviewServiceTests
{
    [Fact]
    public async Task PreviewAsync_BuildsDecempAndPresentAnnexButKeepsOfficialModeBlocked()
    {
        await using var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var client = new ClientCompany
        {
            Code = "CLI01", RaisonSociale = "COMPANY", MatriculeFiscal = "1234567", Cle = "A", Categorie = "B",
            Etablissement = "000", Activite = "ACCOUNTING", Ville = "TUNIS", Adresse = "MAIN STREET",
            NumeroAdresse = "10", CodePostal = "1000"
        };
        var year = new FiscalYear { ClientCompanyId = client.Id, Year = 2025 };
        var declaration = new EmployerDeclaration
        {
            ClientCompanyId = client.Id, FiscalYearId = year.Id, Year = 2025, Title = "Declaration 2025"
        };
        var annex = new DeclarationAnnex { DeclarationId = declaration.Id, AnnexCode = "A1", Title = "A1" };
        var beneficiary = new DeclarationBeneficiary
        {
            DeclarationId = declaration.Id, IdentifierType = BeneficiaryIdentifierType.CIN,
            Identifier = "12345678", FullNameOrCompanyName = "EMPLOYEE", JobTitle = "ACCOUNTANT", Address = "TUNIS"
        };
        var line = new DeclarationLine
        {
            DeclarationId = declaration.Id, AnnexId = annex.Id, BeneficiaryId = beneficiary.Id,
            OrderNumber = 1, OperationType = "EMPCCA-A1", GrossAmount = 1000, TaxableAmount = 1000,
            WithheldAmount = 100,
            AnnexA1Detail = new AnnexA1Detail
            {
                FamilySituation = 2, WorkPeriodStart = new DateOnly(2025, 1, 1),
                WorkPeriodEnd = new DateOnly(2025, 12, 31), WorkPeriodDays = 365,
                TaxableIncome = 1000, GrossTaxableIncome = 1000, CommonRegimeWithheldAmount = 100, NetPaidAmount = 900
            }
        };
        db.AddRange(client, year, declaration, annex, beneficiary, line);
        await db.SaveChangesAsync();

        var service = new EmpccaGenerationPreviewService(db, new FakeCurrentUserService(), new TestHostEnvironment());
        var result = await service.PreviewAsync(declaration.Id);

        result.CanGenerateOfficial.Should().BeFalse();
        result.Files.Should().Contain(x => x.FileName == "DECEMP_25" && x.LineCount == 51 && x.ExpectedLineLength == 38);
        result.Files.Should().Contain(x => x.FileName == "ANXEMP_1_25_1" && x.LineCount == 3 && x.ExpectedLineLength == 399);
        result.BlockingIssues.Should().Contain(x => x.Contains("contradiction de position", StringComparison.Ordinal));
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
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
