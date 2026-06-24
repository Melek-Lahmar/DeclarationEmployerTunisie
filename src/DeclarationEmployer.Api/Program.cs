using DeclarationEmployer.Api.Middleware;
using DeclarationEmployer.Infrastructure;
using DeclarationEmployer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
        fiscalYear = 2025
    });
});

app.MapControllers();

app.Run();
