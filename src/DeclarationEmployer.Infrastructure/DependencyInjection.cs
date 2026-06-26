using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Application.Auth.Validation;
using DeclarationEmployer.Application.Cabinet.Validation;
using DeclarationEmployer.Application.Dashboard;
using DeclarationEmployer.Application.Declarations;
using DeclarationEmployer.Application.Fiscal;
using DeclarationEmployer.FiscalEngine;
using DeclarationEmployer.FiscalEngine.Rules;
using DeclarationEmployer.Import;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeclarationEmployer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "La chaine de connexion 'DefaultConnection' est introuvable.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<DefaultAdminOptions>(configuration.GetSection("DefaultAdmin"));
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        services.AddValidatorsFromAssemblyContaining<CreateClientCompanyRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddSingleton<IFiscalControlRule, GrossAmountMustBePositiveOrZeroRule>();
        services.AddSingleton<IFiscalControlRule, TaxableAmountMustBePositiveOrZeroRule>();
        services.AddSingleton<IFiscalControlRule, WithheldAmountMustBePositiveOrZeroRule>();
        services.AddSingleton<IFiscalControlRule, RateMustBeBetweenZeroAndHundredRule>();
        services.AddSingleton<IFiscalControlRule, WithheldAmountMustNotExceedTaxableAmountRule>();
        services.AddSingleton<IFiscalControlRule, BeneficiaryRequiredRule>();
        services.AddSingleton<IFiscalControlRule, OperationTypeRequiredRule>();
        services.AddSingleton<IFiscalControlRule, PaymentDateMustBeInsideFiscalYearRule>();
        services.AddSingleton<IFiscalControlRule, ZeroRateWarningRule>();
        services.AddSingleton<IFiscalControlRule, MissingDocumentReferenceWarningRule>();
        services.AddSingleton<IFiscalControlRule, ZeroTaxableWithWithheldAmountWarningRule>();
        services.AddSingleton<IFiscalControlRule, MissingFiscalCategoryInfoRule>();
        services.AddSingleton<IFiscalControlEngine, FiscalControlEngine>();
        services.AddSingleton<IExcelDeclarationImportService, ExcelDeclarationImportService>();
        services.AddSingleton<IFileHashService, FileHashService>();
        services.AddSingleton<IDeclarationExportStorageService, DeclarationExportStorageService>();
        services.AddSingleton<IInternalDeclarationCsvGenerator, InternalDeclarationCsvGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITemporaryFileStorageService, TemporaryFileStorageService>();
        services.AddScoped<DevelopmentAdminSeedService>();
        services.AddScoped<FiscalReferenceSeedService>();
        services.AddScoped<IClientsService, ClientsService>();
        services.AddScoped<IFiscalYearsService, FiscalYearsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDeclarationsService, DeclarationsService>();
        services.AddScoped<IAnnexA1Service, AnnexA1Service>();
        services.AddScoped<IAnnexFoundationService, AnnexFoundationService>();
        services.AddScoped<IDeclarationAnnexesService, DeclarationAnnexesService>();
        services.AddScoped<IDeclarationBeneficiariesService, DeclarationBeneficiariesService>();
        services.AddScoped<IDeclarationLinesService, DeclarationLinesService>();
        services.AddScoped<IDeclarationAnomaliesService, DeclarationAnomaliesService>();
        services.AddScoped<IGeneratedFilesService, GeneratedFilesService>();
        services.AddScoped<IArchivedDocumentsService, ArchivedDocumentsService>();
        services.AddScoped<IDeclarationImportService, DeclarationImportService>();
        services.AddScoped<IDeclarationControlService, DeclarationControlService>();
        services.AddScoped<IDeclarationExportService, DeclarationExportService>();
        services.AddScoped<IGenerationService, GenerationService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IFiscalReferenceService, FiscalReferenceService>();

        return services;
    }
}
