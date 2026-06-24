using DeclarationEmployer.Domain.Audit;
using DeclarationEmployer.Domain.Auth;
using DeclarationEmployer.Domain.Cabinet;
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
