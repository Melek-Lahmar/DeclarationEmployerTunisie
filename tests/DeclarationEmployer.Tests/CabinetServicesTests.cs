using DeclarationEmployer.Application.Cabinet.Validation;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Tests;

public sealed class CabinetServicesTests
{
    [Fact]
    public async Task ClientsService_CreateAsync_NormalizesCodeAndWritesAudit()
    {
        await using var db = CreateDbContext();
        var service = new ClientsService(
            db,
            new CreateClientCompanyRequestValidator(),
            new UpdateClientCompanyRequestValidator());

        var client = await service.CreateAsync(
            new CreateClientCompanyRequest
            {
                Code = " cli01 ",
                RaisonSociale = " Societe Test ",
                Ville = "Sfax"
            },
            "127.0.0.1");

        client.Code.Should().Be("CLI01");
        client.RaisonSociale.Should().Be("Societe Test");
        db.AuditLogs.Should().ContainSingle(x => x.Action == "CLIENT_CREATED");

        var summary = await service.GetSummaryAsync(client.Id);
        summary.Should().NotBeNull();
        summary!.Client.Code.Should().Be("CLI01");
        summary.LastAuditAction.Should().Be("CLIENT_CREATED");

        var searchResult = await service.GetAllAsync(
            includeInactive: false,
            search: "sfax",
            status: "active");
        searchResult.Should().ContainSingle(x => x.Code == "CLI01");

        var history = await service.GetHistoryAsync(client.Id);
        history.Should().ContainSingle(x => x.Action == "CLIENT_CREATED");
    }

    [Fact]
    public async Task ClientsService_CreateAsync_RejectsDuplicateCode()
    {
        await using var db = CreateDbContext();
        var service = new ClientsService(
            db,
            new CreateClientCompanyRequestValidator(),
            new UpdateClientCompanyRequestValidator());

        var request = new CreateClientCompanyRequest
        {
            Code = "CLI01",
            RaisonSociale = "Societe Test"
        };

        await service.CreateAsync(request, null);
        var act = () => service.CreateAsync(request, null);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task FiscalYearsService_CloseAndReopen_UpdatesStatusAndWritesAudit()
    {
        await using var db = CreateDbContext();
        var client = new ClientCompany
        {
            Id = Guid.NewGuid(),
            Code = "CLI01",
            RaisonSociale = "Societe Test",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var service = new FiscalYearsService(
            db,
            new CreateFiscalYearRequestValidator(),
            new UpdateFiscalYearRequestValidator());

        var fiscalYear = await service.CreateAsync(
            client.Id,
            new CreateFiscalYearRequest { Year = 2025 },
            null);

        var closed = await service.CloseAsync(fiscalYear.Id, null);
        closed.IsClosed.Should().BeTrue();
        closed.ClosedAt.Should().NotBeNull();

        var updateClosedAct = () => service.UpdateAsync(
            fiscalYear.Id,
            new UpdateFiscalYearRequest { Year = 2026 },
            null);
        await updateClosedAct.Should().ThrowAsync<ApplicationConflictException>();

        var reopenWithoutReasonAct = () => service.ReopenAsync(
            fiscalYear.Id,
            new ReopenFiscalYearRequest { Reason = " " },
            null);
        await reopenWithoutReasonAct.Should().ThrowAsync<ApplicationConflictException>();

        var reopened = await service.ReopenAsync(
            fiscalYear.Id,
            new ReopenFiscalYearRequest { Reason = "Correction apres controle interne." },
            null);
        reopened.IsClosed.Should().BeFalse();
        reopened.ClosedAt.Should().BeNull();

        db.AuditLogs.Should().Contain(x => x.Action == "FISCAL_YEAR_CREATED");
        db.AuditLogs.Should().Contain(x => x.Action == "FISCAL_YEAR_CLOSED");
        db.AuditLogs.Should().Contain(x => x.Action == "FISCAL_YEAR_REOPENED");

        var history = await service.GetHistoryAsync(fiscalYear.Id);
        history.Should().HaveCount(3);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
