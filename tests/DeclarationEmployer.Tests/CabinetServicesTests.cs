using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Auth.Validation;
using DeclarationEmployer.Application.Cabinet.Validation;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Application.Declarations.Validation;
using DeclarationEmployer.Contracts.Auth;
using DeclarationEmployer.Contracts.Cabinet;
using DeclarationEmployer.Contracts.Declarations;
using DeclarationEmployer.Contracts.Users;
using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Tests;

public sealed class CabinetServicesTests
{
    [Fact]
    public async Task ClientsService_CreateAsync_NormalizesCodeAndWritesAudit()
    {
        await using var db = CreateDbContext();
        var service = new ClientsService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateClientCompanyRequestValidator(),
            new UpdateClientCompanyRequestValidator());

        var client = await service.CreateAsync(
            new CreateClientCompanyRequest
            {
                Code = " cli01 ",
                RaisonSociale = " Societe Test ",
                MatriculeFiscal = "1234567",
                Cle = "a",
                Categorie = "m",
                Activite = "Services",
                Adresse = "Route de Tunis",
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
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateClientCompanyRequestValidator(),
            new UpdateClientCompanyRequestValidator());

        var request = new CreateClientCompanyRequest
        {
            Code = "CLI01",
            RaisonSociale = "Societe Test",
            MatriculeFiscal = "1234567",
            Cle = "A",
            Categorie = "M",
            Activite = "Services",
            Adresse = "Route de Tunis",
            Ville = "Sfax"
        };

        await service.CreateAsync(request, null);
        var act = () => service.CreateAsync(request, null);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task CreateClient_WithValidReferenceFields_ShouldCreateClientAndDefaultEstablishment()
    {
        await using var db = CreateDbContext();
        var service = CreateClientsService(db);

        var client = await service.CreateAsync(ValidClientRequest(), null);

        client.MatriculeFiscal.Should().Be("7654321");
        client.Cle.Should().Be("B");
        client.Categorie.Should().Be("M");
        client.Etablissement.Should().Be("000");
        client.Adresse.Should().Be("RTE EL AIN KM 3");
        client.NumeroAdresse.Should().Be("0");
    }

    [Theory]
    [InlineData("", "SOCIETE TEST", "7654321", "B", "M")]
    [InlineData("TEST002", "", "7654321", "B", "M")]
    [InlineData("TEST002", "SOCIETE TEST", "ABC", "B", "M")]
    [InlineData("TEST002", "SOCIETE TEST", "7654321", "B", "E")]
    public async Task CreateClient_WithInvalidFiscalReference_ShouldFail(
        string code, string name, string identifier, string key, string category)
    {
        await using var db = CreateDbContext();
        var request = ValidClientRequest();
        request.Code = code;
        request.RaisonSociale = name;
        request.MatriculeFiscal = identifier;
        request.Cle = key;
        request.Categorie = category;

        var act = () => CreateClientsService(db).CreateAsync(request, null);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task UpdateDeactivateSearchAndFilterClient_ShouldPersistExpectedState()
    {
        await using var db = CreateDbContext();
        var service = CreateClientsService(db);
        var created = await service.CreateAsync(ValidClientRequest(), null);

        var updated = await service.UpdateAsync(created.Id, new UpdateClientCompanyRequest
        {
            Code = created.Code,
            RaisonSociale = created.RaisonSociale,
            MatriculeFiscal = created.MatriculeFiscal,
            Cle = created.Cle,
            Categorie = created.Categorie,
            CodeTva = "A",
            Etablissement = "0",
            Activite = "SERVICES INFORMATIQUES",
            Ville = "SFAX",
            Adresse = "RTE GREMDA KM 2",
            NumeroAdresse = null,
            CodePostal = "3012",
            Telephone = "22222222",
            IsActive = true
        }, null);

        updated.Adresse.Should().Be("RTE GREMDA KM 2");
        updated.Telephone.Should().Be("22222222");
        updated.Etablissement.Should().Be("000");
        updated.NumeroAdresse.Should().Be("0");
        (await service.GetAllAsync(false, "TEST002", "active")).Should().ContainSingle();
        (await service.GetAllAsync(false, "SOCIETE TEST", "active")).Should().ContainSingle();
        (await service.GetAllAsync(false, "7654321", "active")).Should().ContainSingle();
        (await service.GetAllAsync(false, "SFAX", "active")).Should().ContainSingle();

        await service.DeactivateAsync(created.Id, null);

        (await service.GetAllAsync(false, null, "active")).Should().BeEmpty();
        (await service.GetAllAsync(true, null, "inactive")).Should().ContainSingle(x => !x.IsActive);
        db.AuditLogs.Should().Contain(x => x.Action == "CLIENT_DEACTIVATED");
    }

    [Fact]
    public async Task UpdateClient_ShouldKeepLeadingZeroIdentifier_AndPersistModifiedFields()
    {
        await using var db = CreateDbContext();
        var service = CreateClientsService(db);
        var created = await service.CreateAsync(new CreateClientCompanyRequest
        {
            Code = "2",
            RaisonSociale = "STE MARWA DE CONFECTION",
            MatriculeFiscal = "0580165",
            Cle = "Y",
            Categorie = "M",
            CodeTva = "A",
            Etablissement = "000",
            Activite = "FAB.ARTICLES EN TEXTILE",
            Adresse = "000 RTE EL AIN KM 3 SFAX 3051",
            Ville = "SFAX",
            NumeroAdresse = "0",
            CodePostal = "3012",
            Telephone = "98415573"
        }, null);

        var updated = await service.UpdateAsync(created.Id, new UpdateClientCompanyRequest
        {
            Code = "2",
            RaisonSociale = "STE MARWA DE CONFECTION",
            MatriculeFiscal = "0580165",
            Cle = "Y",
            Categorie = "M",
            CodeTva = "A",
            Etablissement = "0",
            Activite = "FAB.ARTICLES EN TEXTILE",
            Adresse = "000 RTE EL AIN KM 3 SFAX 3051 MODIFIE",
            Ville = "SFAX",
            NumeroAdresse = "0",
            CodePostal = "3012",
            Telephone = "98415574",
            IsActive = true
        }, null);

        updated.MatriculeFiscal.Should().Be("0580165");
        updated.Etablissement.Should().Be("000");
        updated.CodePostal.Should().Be("3012");
        updated.Adresse.Should().Be("000 RTE EL AIN KM 3 SFAX 3051 MODIFIE");
        updated.Telephone.Should().Be("98415574");
    }

    [Fact]
    public async Task DeleteClient_WithoutDeclarations_ShouldDelete()
    {
        await using var db = CreateDbContext();
        var service = CreateClientsService(db);
        var created = await service.CreateAsync(ValidClientRequest(), null);

        await service.DeleteAsync(created.Id, null);

        (await service.GetByIdAsync(created.Id)).Should().BeNull();
        db.AuditLogs.Should().Contain(x => x.Action == "CLIENT_DELETED");
    }

    [Fact]
    public async Task DeleteClient_WithDeclarations_ShouldReturnBusinessError()
    {
        await using var db = CreateDbContext();
        var service = CreateClientsService(db);
        var client = await service.CreateAsync(ValidClientRequest(), null);
        var fiscalYear = new FiscalYear
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            Year = 2025,
            IsClosed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.FiscalYears.Add(fiscalYear);
        db.Declarations.Add(new EmployerDeclaration
        {
            Id = Guid.NewGuid(),
            ClientCompanyId = client.Id,
            FiscalYearId = fiscalYear.Id,
            Year = 2025,
            ActCode = DeclarationActCode.Spontaneous,
            Status = DeclarationStatus.Draft,
            Title = "Declaration test",
            IsLocked = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var act = () => service.DeleteAsync(client.Id, null);

        await act.Should()
            .ThrowAsync<ApplicationConflictException>()
            .WithMessage("*Elle ne peut pas être supprimée*");
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
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
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

    [Fact]
    public async Task DashboardService_GetSummaryAndRecentActions_ReturnsExistingDataOnly()
    {
        await using var db = CreateDbContext();
        var clientId = Guid.NewGuid();
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
            Id = Guid.NewGuid(),
            ClientCompanyId = clientId,
            Year = 2025,
            IsClosed = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = "CLIENT_CREATED",
            EntityName = nameof(ClientCompany),
            Details = "Creation test",
            OccurredAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new DashboardService(db);

        var summary = await service.GetSummaryAsync();
        summary.ClientsCount.Should().Be(1);
        summary.ActiveClientsCount.Should().Be(1);
        summary.FiscalYearsCount.Should().Be(1);
        summary.OpenFiscalYearsCount.Should().Be(1);
        summary.BlockingAnomaliesCount.Should().Be(0);

        var actions = await service.GetRecentActionsAsync(10);
        actions.Should().ContainSingle(x => x.Action == "CLIENT_CREATED");
    }

    [Fact]
    public async Task DeclarationsService_CreateCloseAndPreventUpdate_WritesEventsAndAudit()
    {
        await using var db = CreateDbContext();
        var clientId = Guid.NewGuid();
        var fiscalYearId = Guid.NewGuid();

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
        await db.SaveChangesAsync();

        var service = new DeclarationsService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateDeclarationRequestValidator(),
            new UpdateDeclarationRequestValidator());

        var declaration = await service.CreateAsync(
            new CreateDeclarationRequest
            {
                ClientCompanyId = clientId,
                FiscalYearId = fiscalYearId
            },
            null);

        declaration.Status.Should().Be(DeclarationStatus.Draft.ToString());
        declaration.Title.Should().Be("Declaration employeur 2025 - Societe Test");
        db.DeclarationAnnexes.Should().HaveCount(7);
        db.AuditLogs.Should().Contain(x => x.Action == "DECLARATION_CREATED");

        var duplicateAct = () => service.CreateAsync(
            new CreateDeclarationRequest
            {
                ClientCompanyId = clientId,
                FiscalYearId = fiscalYearId
            },
            null);
        await duplicateAct.Should().ThrowAsync<ApplicationConflictException>();

        var events = await service.GetEventsAsync(declaration.Id);
        events.Should().ContainSingle(x => x.Action == "DECLARATION_CREATED");

        var closed = await service.CloseAsync(declaration.Id, null);
        closed.Status.Should().Be(DeclarationStatus.Closed.ToString());
        closed.IsLocked.Should().BeTrue();

        var updateClosedAct = () => service.UpdateAsync(
            declaration.Id,
            new UpdateDeclarationRequest { Title = "Declaration modifiee" },
            null);
        await updateClosedAct.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task AuthService_LoginAsync_ReturnsTokenForValidCredentials()
    {
        await using var db = CreateDbContext();
        var passwordService = new PasswordService();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@local.dev",
            PasswordHash = passwordService.HashPassword("ChangeMe123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new AuthService(
            db,
            passwordService,
            CreateJwtTokenService(),
            new LoginRequestValidator());

        var response = await service.LoginAsync(new LoginRequest
        {
            UserNameOrEmail = "admin",
            Password = "ChangeMe123!"
        });

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.User.UserName.Should().Be("admin");
        response.User.Role.Should().Be(UserRole.Admin.ToString());
    }

    [Fact]
    public async Task AuthService_LoginAsync_RejectsInactiveUser()
    {
        await using var db = CreateDbContext();
        var passwordService = new PasswordService();
        db.Users.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "viewer",
            Email = "viewer@local.dev",
            PasswordHash = passwordService.HashPassword("ChangeMe123!"),
            Role = UserRole.Viewer,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new AuthService(
            db,
            passwordService,
            CreateJwtTokenService(),
            new LoginRequestValidator());

        var act = () => service.LoginAsync(new LoginRequest
        {
            UserNameOrEmail = "viewer",
            Password = "ChangeMe123!"
        });

        await act.Should().ThrowAsync<ApplicationUnauthorizedException>();
    }

    [Fact]
    public async Task UsersService_CreateAsync_HashesPasswordAndWritesAuditWithCurrentUser()
    {
        await using var db = CreateDbContext();
        var passwordService = new PasswordService();
        var service = new UsersService(
            db,
            passwordService,
            new FakeCurrentUserService(true, "admin", Guid.NewGuid(), UserRole.Admin.ToString()),
            new TestHostEnvironment(),
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator(),
            new ChangePasswordRequestValidator());

        var user = await service.CreateAsync(new CreateUserRequest
        {
            UserName = "manager",
            Email = "manager@local.dev",
            Password = "ChangeMe123!",
            Role = UserRole.Manager.ToString()
        }, "127.0.0.1");

        user.UserName.Should().Be("manager");
        user.Role.Should().Be(UserRole.Manager.ToString());
        db.Users.Single().PasswordHash.Should().NotBe("ChangeMe123!");
        passwordService.VerifyPassword("ChangeMe123!", db.Users.Single().PasswordHash).Should().BeTrue();
        db.AuditLogs.Should().ContainSingle(x => x.Action == "USER_CREATED" && x.UserName == "admin");
    }

    [Fact]
    public async Task UsersService_CreateAsync_RejectsDuplicateEmail()
    {
        await using var db = CreateDbContext();
        var passwordService = new PasswordService();
        db.Users.Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@local.dev",
            PasswordHash = passwordService.HashPassword("ChangeMe123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new UsersService(
            db,
            passwordService,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateUserRequestValidator(),
            new UpdateUserRequestValidator(),
            new ChangePasswordRequestValidator());

        var act = () => service.CreateAsync(new CreateUserRequest
        {
            UserName = "manager",
            Email = "admin@local.dev",
            Password = "ChangeMe123!",
            Role = UserRole.Manager.ToString()
        }, null);

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task UserDto_DoesNotContainPasswordHash()
    {
        typeof(UserDto).GetProperty("PasswordHash").Should().BeNull();
    }

    private static ClientsService CreateClientsService(ApplicationDbContext db)
    {
        return new ClientsService(
            db,
            new FakeCurrentUserService(),
            new TestHostEnvironment(),
            new CreateClientCompanyRequestValidator(),
            new UpdateClientCompanyRequestValidator());
    }

    private static CreateClientCompanyRequest ValidClientRequest()
    {
        return new CreateClientCompanyRequest
        {
            Code = "TEST002",
            RaisonSociale = "SOCIETE TEST CHAMPS REFERENCE",
            MatriculeFiscal = "7654321",
            Cle = "B",
            Categorie = "M",
            CodeTva = "A",
            Etablissement = "",
            Activite = "SERVICES INFORMATIQUES",
            Ville = "SFAX",
            Adresse = "RTE EL AIN KM 3",
            NumeroAdresse = "0",
            CodePostal = "3012",
            Telephone = "98415573"
        };
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static JwtTokenService CreateJwtTokenService()
    {
        return new JwtTokenService(Options.Create(new Infrastructure.Configuration.JwtOptions
        {
            Issuer = "DeclarationEmployerTunisie",
            Audience = "DeclarationEmployerTunisie.Desktop",
            Secret = "ChangeThisExampleSigningKeyOnlyForLocalDev123456789",
            ExpirationMinutes = 60
        }));
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public FakeCurrentUserService(
            bool isAuthenticated = false,
            string? userName = null,
            Guid? userId = null,
            string? role = null)
        {
            IsAuthenticated = isAuthenticated;
            UserName = userName;
            UserId = userId;
            Role = role;
        }

        public Guid? UserId { get; }

        public string? UserName { get; }

        public string? Role { get; }

        public bool IsAuthenticated { get; }
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
