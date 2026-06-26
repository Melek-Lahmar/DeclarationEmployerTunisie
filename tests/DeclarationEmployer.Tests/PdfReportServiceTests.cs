using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Tests;

public sealed class PdfReportServiceTests
{
    [Fact]
    public async Task BuildDeclarationSummaryAsync_ReturnsPdfBytes()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = new PdfReportService(db);

        var report = await service.BuildDeclarationSummaryAsync(declarationId);

        report.FileName.Should().EndWith(".pdf");
        report.Content.Length.Should().BeGreaterThan(1000);
        System.Text.Encoding.ASCII.GetString(report.Content.Take(4).ToArray()).Should().Be("%PDF");
    }

    private static async Task<Guid> SeedDeclarationAsync(ApplicationDbContext db)
    {
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe PDF",
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
            Title = "Declaration PDF",
            Status = DeclarationStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Clients.Add(client);
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(declaration);
        await db.SaveChangesAsync();

        return declaration.Id;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
