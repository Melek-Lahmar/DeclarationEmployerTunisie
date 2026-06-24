using DeclarationEmployer.Application.Cabinet;
using DeclarationEmployer.Application.Cabinet.Validation;
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

        services.AddValidatorsFromAssemblyContaining<CreateClientCompanyRequestValidator>();
        services.AddScoped<IClientsService, ClientsService>();
        services.AddScoped<IFiscalYearsService, FiscalYearsService>();

        return services;
    }
}
