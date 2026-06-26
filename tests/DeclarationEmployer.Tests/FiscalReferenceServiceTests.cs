using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Tests;

public sealed class FiscalReferenceServiceTests
{
    [Fact]
    public async Task Seed_CreatesUnconfirmedRuleSetAndAnnexesFor2025()
    {
        await using var db = CreateDbContext();
        var seed = new FiscalReferenceSeedService(db);

        await seed.EnsureSeededAsync();

        db.FiscalRuleSets.Should().ContainSingle(x => x.Year == 2025 && x.Code == "EMPCCA-2025-FOUNDATION");
        db.AnnexDefinitions.Should().HaveCount(7);
        db.AnnexDefinitions.Should().OnlyContain(x => !x.IsOfficialMappingConfirmed);
        db.FiscalFieldDefinitions.Should().OnlyContain(x => !x.IsConfirmed);
    }

    [Fact]
    public async Task GetReadinessAsync_DisablesOfficialGenerationWhenMappingIsUnconfirmed()
    {
        await using var db = CreateDbContext();
        await new FiscalReferenceSeedService(db).EnsureSeededAsync();
        var service = new FiscalReferenceService(db);

        var readiness = await service.GetReadinessAsync(2025);

        readiness.HasActiveRuleSet.Should().BeTrue();
        readiness.RuleSetCode.Should().Be("EMPCCA-2025-FOUNDATION");
        readiness.AnnexesCount.Should().Be(7);
        readiness.ConfirmedAnnexesCount.Should().Be(0);
        readiness.IsOfficialGenerationEnabled.Should().BeFalse();
        readiness.Message.Should().Be(FiscalReferenceSeedService.OfficialMappingIncompleteMessage);
    }

    [Fact]
    public async Task GetFieldsAsync_ReturnsFoundationFieldsForAnnexA1()
    {
        await using var db = CreateDbContext();
        await new FiscalReferenceSeedService(db).EnsureSeededAsync();
        var service = new FiscalReferenceService(db);

        var fields = await service.GetFieldsAsync(2025, "a1");

        fields.Should().Contain(x => x.Code == "BeneficiaryIdentifier" && x.IsRequired);
        fields.Should().Contain(x => x.Code == "PeriodStart");
        fields.Should().OnlyContain(x => x.IsConfirmed == false);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
