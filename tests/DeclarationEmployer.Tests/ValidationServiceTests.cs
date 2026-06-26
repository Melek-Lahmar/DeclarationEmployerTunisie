using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Contracts.Validation;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DeclarationEmployer.Tests;

public sealed class ValidationServiceTests
{
    [Fact]
    public async Task RunAsync_CreatesValidationRunAndResultsFromControl()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var service = CreateService(db, CreateControlServiceResult(declarationId));

        var result = await service.RunAsync(declarationId);

        result.Run.DeclarationId.Should().Be(declarationId);
        result.Run.BlockingCount.Should().Be(1);
        result.Run.WarningCount.Should().Be(1);
        result.Run.Score.Should().Be(75);
        result.Results.Should().HaveCount(2);
        db.ValidationRuns.Should().ContainSingle();
        db.ValidationResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task IgnoreAsync_RequiresJustification()
    {
        await using var db = CreateDbContext();
        var declarationId = await SeedDeclarationAsync(db);
        var validationResult = new ValidationResult
        {
            Id = Guid.NewGuid(),
            ValidationRunId = Guid.NewGuid(),
            DeclarationId = declarationId,
            Severity = DeclarationAnomalySeverity.Warning,
            Code = "WARN",
            Message = "Warning",
            Status = ValidationResultStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.ValidationRuns.Add(new ValidationRun
        {
            Id = validationResult.ValidationRunId,
            DeclarationId = declarationId,
            Status = ValidationRunStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        });
        db.ValidationResults.Add(validationResult);
        await db.SaveChangesAsync();

        var service = CreateService(db, CreateControlServiceResult(declarationId));

        var act = () => service.IgnoreAsync(validationResult.Id, new IgnoreValidationResultRequest { Justification = " " });

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    private static DeclarationControlResultDto CreateControlServiceResult(Guid declarationId)
    {
        return new DeclarationControlResultDto
        {
            DeclarationId = declarationId,
            CheckedLinesCount = 1,
            BlockingAnomaliesCount = 1,
            WarningAnomaliesCount = 1,
            InfoAnomaliesCount = 0,
            DeclarationStatus = DeclarationStatus.Imported.ToString(),
            Anomalies =
            [
                new DeclarationAnomalyDto
                {
                    Id = Guid.NewGuid(),
                    DeclarationId = declarationId,
                    Severity = DeclarationAnomalySeverity.Blocking.ToString(),
                    Code = "BLOCK",
                    Message = "Blocking issue",
                    EntityName = nameof(DeclarationLine),
                    EntityId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow
                },
                new DeclarationAnomalyDto
                {
                    Id = Guid.NewGuid(),
                    DeclarationId = declarationId,
                    Severity = DeclarationAnomalySeverity.Warning.ToString(),
                    Code = "WARN",
                    Message = "Warning issue",
                    EntityName = nameof(DeclarationLine),
                    EntityId = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]
        };
    }

    private static ValidationService CreateService(
        ApplicationDbContext db,
        DeclarationControlResultDto controlResult)
    {
        return new ValidationService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new FakeDeclarationControlService(controlResult));
    }

    private static async Task<Guid> SeedDeclarationAsync(ApplicationDbContext db)
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

    private sealed class FakeDeclarationControlService : IDeclarationControlService
    {
        private readonly DeclarationControlResultDto _controlResult;

        public FakeDeclarationControlService(DeclarationControlResultDto controlResult)
        {
            _controlResult = controlResult;
        }

        public Task<DeclarationControlResultDto> ControlAsync(Guid declarationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_controlResult);
        }

        public Task<IReadOnlyList<DeclarationAnomalyDto>> GetAnomaliesAsync(Guid declarationId, string? severity, bool includeResolved, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_controlResult.Anomalies);
        }

        public Task<DeclarationAnomalyDto> ResolveAnomalyAsync(Guid declarationId, Guid anomalyId, ResolveDeclarationAnomalyRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
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
