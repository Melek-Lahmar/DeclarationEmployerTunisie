using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
using DeclarationEmployer.Domain.Declarations;
using Microsoft.EntityFrameworkCore;

namespace DeclarationEmployer.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<ClientCompany> Clients => Set<ClientCompany>();

    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();

    public DbSet<EmployerDeclaration> Declarations => Set<EmployerDeclaration>();

    public DbSet<DeclarationEvent> DeclarationEvents => Set<DeclarationEvent>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users", "auth");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(x => x.LastLoginAt)
                .HasColumnName("last_login_at");

            entity.HasIndex(x => x.UserName)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<ClientCompany>(entity =>
        {
            entity.ToTable("clients", "cabinet");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Code)
                .HasColumnName("code")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RaisonSociale)
                .HasColumnName("raison_sociale")
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.MatriculeFiscal)
                .HasColumnName("matricule_fiscal")
                .HasMaxLength(20);

            entity.Property(x => x.Cle)
                .HasColumnName("cle")
                .HasMaxLength(5);

            entity.Property(x => x.Categorie)
                .HasColumnName("categorie")
                .HasMaxLength(5);

            entity.Property(x => x.CodeTva)
                .HasColumnName("code_tva")
                .HasMaxLength(5);

            entity.Property(x => x.Etablissement)
                .HasColumnName("etablissement")
                .HasMaxLength(10);

            entity.Property(x => x.Activite)
                .HasColumnName("activite")
                .HasMaxLength(250);

            entity.Property(x => x.Adresse)
                .HasColumnName("adresse")
                .HasMaxLength(500);

            entity.Property(x => x.Ville)
                .HasColumnName("ville")
                .HasMaxLength(100);

            entity.Property(x => x.CodePostal)
                .HasColumnName("code_postal")
                .HasMaxLength(20);

            entity.Property(x => x.Telephone)
                .HasColumnName("telephone")
                .HasMaxLength(50);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasIndex(x => x.MatriculeFiscal);
        });

        modelBuilder.Entity<FiscalYear>(entity =>
        {
            entity.ToTable("fiscal_years", "cabinet");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ClientCompanyId)
                .HasColumnName("client_company_id")
                .IsRequired();

            entity.Property(x => x.Year)
                .HasColumnName("year")
                .IsRequired();

            entity.Property(x => x.IsClosed)
                .HasColumnName("is_closed")
                .IsRequired();

            entity.Property(x => x.ClosedAt)
                .HasColumnName("closed_at");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasOne(x => x.ClientCompany)
                .WithMany()
                .HasForeignKey(x => x.ClientCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ClientCompanyId, x.Year })
                .IsUnique();
        });

        modelBuilder.Entity<EmployerDeclaration>(entity =>
        {
            entity.ToTable("declarations", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ClientCompanyId)
                .HasColumnName("client_company_id")
                .IsRequired();

            entity.Property(x => x.FiscalYearId)
                .HasColumnName("fiscal_year_id")
                .IsRequired();

            entity.Property(x => x.Year)
                .HasColumnName("year")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasColumnName("notes")
                .HasMaxLength(1000);

            entity.Property(x => x.IsLocked)
                .HasColumnName("is_locked")
                .IsRequired();

            entity.Property(x => x.LockedAt)
                .HasColumnName("locked_at");

            entity.Property(x => x.LockedBy)
                .HasColumnName("locked_by")
                .HasMaxLength(100);

            entity.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

            entity.HasOne(x => x.ClientCompany)
                .WithMany()
                .HasForeignKey(x => x.ClientCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FiscalYear)
                .WithMany()
                .HasForeignKey(x => x.FiscalYearId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ClientCompanyId, x.FiscalYearId })
                .IsUnique();

            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DeclarationEvent>(entity =>
        {
            entity.ToTable("declaration_events", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.DeclarationId)
                .HasColumnName("declaration_id")
                .IsRequired();

            entity.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000);

            entity.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100);

            entity.Property(x => x.OccurredAt)
                .HasColumnName("occurred_at")
                .IsRequired();

            entity.HasOne(x => x.Declaration)
                .WithMany()
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => x.OccurredAt);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs", "audit");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Action)
                .HasColumnName("action")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.EntityName)
                .HasColumnName("entity_name")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.EntityId)
                .HasColumnName("entity_id")
                .HasMaxLength(100);

            entity.Property(x => x.UserName)
                .HasColumnName("user_name")
                .HasMaxLength(100);

            entity.Property(x => x.Details)
                .HasColumnName("details")
                .HasMaxLength(4000);

            entity.Property(x => x.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(100);

            entity.Property(x => x.OccurredAt)
                .HasColumnName("occurred_at")
                .IsRequired();

            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => x.EntityName);
            entity.HasIndex(x => x.Action);
        });
    }
}
