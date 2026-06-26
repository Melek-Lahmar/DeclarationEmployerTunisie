using DeclarationEmployer.Api.Middleware;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure;
using DeclarationEmployer.Infrastructure.Services;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "C:/DET2025_DEV/logs/api-.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    jwtOptions.Issuer = "DeclarationEmployerTunisie";
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    jwtOptions.Audience = "DeclarationEmployerTunisie.Desktop";
}

if (jwtOptions.ExpirationMinutes <= 0)
{
    jwtOptions.ExpirationMinutes = 60;
}

if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtOptions.Secret = "ChangeThisExampleSigningKeyOnlyForLocalDev123456789";
    }
    else
    {
        throw new InvalidOperationException(
            "La configuration JWT est incomplete. La cle secrete est obligatoire hors Developpement.");
    }
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DevelopmentAdminSeedService>();
    await seedService.EnsureSeededAsync();

    var fiscalSeedService = scope.ServiceProvider.GetRequiredService<FiscalReferenceSeedService>();
    await fiscalSeedService.EnsureSeededAsync();
}

app.MapGet("/api/health", async (ApplicationDbContext db) =>
{
    try
    {
        var databaseCanConnect = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            status = "OK",
            application = "Declaration Employeur Tunisie",
            api = "Running",
            database = databaseCanConnect ? "Connected" : "Disconnected",
            provider = "PostgreSQL",
            timestamp = DateTimeOffset.Now
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Health check failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        name = "Declaration Employeur Tunisie",
        version = "0.1.0",
        type = "Local Desktop + Local API",
        database = "PostgreSQL",
        fiscalYear = 2025,
        officialGenerationEnabled = false,
        fiscalMappingMessage = "Génération officielle non activée : mapping EMPCCA 2025 incomplet ou non confirmé."
    });
});

app.MapControllers();

app.Run();
