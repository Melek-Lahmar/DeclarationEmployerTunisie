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

    public DbSet<DeclarationAnnex> DeclarationAnnexes => Set<DeclarationAnnex>();

    public DbSet<DeclarationBeneficiary> DeclarationBeneficiaries => Set<DeclarationBeneficiary>();

    public DbSet<DeclarationLine> DeclarationLines => Set<DeclarationLine>();

    public DbSet<DeclarationAnomaly> DeclarationAnomalies => Set<DeclarationAnomaly>();

    public DbSet<GeneratedFile> GeneratedFiles => Set<GeneratedFile>();

    public DbSet<ArchivedDocument> ArchivedDocuments => Set<ArchivedDocument>();

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

            entity.Property(x => x.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(50)
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

        modelBuilder.Entity<DeclarationAnnex>(entity =>
        {
            entity.ToTable("declaration_annexes", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.AnnexCode).HasColumnName("annex_code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(250).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.Annexes)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => new { x.DeclarationId, x.AnnexCode });
        });

        modelBuilder.Entity<DeclarationBeneficiary>(entity =>
        {
            entity.ToTable("declaration_beneficiaries", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.IdentifierType).HasColumnName("identifier_type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.Identifier).HasColumnName("identifier").HasMaxLength(100).IsRequired();
            entity.Property(x => x.FullNameOrCompanyName).HasColumnName("full_name_or_company_name").HasMaxLength(250).IsRequired();
            entity.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
            entity.Property(x => x.IsResident).HasColumnName("is_resident").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.Beneficiaries)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => new { x.DeclarationId, x.Identifier });
        });

        modelBuilder.Entity<DeclarationLine>(entity =>
        {
            entity.ToTable("declaration_lines", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.AnnexId).HasColumnName("annex_id");
            entity.Property(x => x.BeneficiaryId).HasColumnName("beneficiary_id");
            entity.Property(x => x.OperationType).HasColumnName("operation_type").HasMaxLength(100).IsRequired();
            entity.Property(x => x.FiscalCategory).HasColumnName("fiscal_category").HasMaxLength(100);
            entity.Property(x => x.GrossAmount).HasColumnName("gross_amount").HasPrecision(18, 3).IsRequired();
            entity.Property(x => x.TaxableAmount).HasColumnName("taxable_amount").HasPrecision(18, 3).IsRequired();
            entity.Property(x => x.Rate).HasColumnName("rate").HasPrecision(9, 4).IsRequired();
            entity.Property(x => x.WithheldAmount).HasColumnName("withheld_amount").HasPrecision(18, 3).IsRequired();
            entity.Property(x => x.PaymentDate).HasColumnName("payment_date");
            entity.Property(x => x.DocumentReference).HasColumnName("document_reference").HasMaxLength(100);
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Annex)
                .WithMany()
                .HasForeignKey(x => x.AnnexId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Beneficiary)
                .WithMany()
                .HasForeignKey(x => x.BeneficiaryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => x.BeneficiaryId);
            entity.HasIndex(x => x.AnnexId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DeclarationAnomaly>(entity =>
        {
            entity.ToTable("declaration_anomalies", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.Severity).HasColumnName("severity").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(150);
            entity.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(100);
            entity.Property(x => x.IsResolved).HasColumnName("is_resolved").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(x => x.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(100);

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.Anomalies)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => x.Severity);
            entity.HasIndex(x => x.IsResolved);
        });

        modelBuilder.Entity<GeneratedFile>(entity =>
        {
            entity.ToTable("generated_files", "declaration");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.FileType).HasColumnName("file_type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(250).IsRequired();
            entity.Property(x => x.RelativePath).HasColumnName("relative_path").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Sha256Hash).HasColumnName("sha256_hash").HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100);

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.GeneratedFiles)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => x.FileType);
        });

        modelBuilder.Entity<ArchivedDocument>(entity =>
        {
            entity.ToTable("archived_documents", "archive");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DeclarationId).HasColumnName("declaration_id").IsRequired();
            entity.Property(x => x.ClientCompanyId).HasColumnName("client_company_id").IsRequired();
            entity.Property(x => x.FiscalYearId).HasColumnName("fiscal_year_id").IsRequired();
            entity.Property(x => x.DocumentType).HasColumnName("document_type").HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(250).IsRequired();
            entity.Property(x => x.RelativePath).HasColumnName("relative_path").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Sha256Hash).HasColumnName("sha256_hash").HasMaxLength(128);
            entity.Property(x => x.ArchivedAt).HasColumnName("archived_at").IsRequired();
            entity.Property(x => x.ArchivedBy).HasColumnName("archived_by").HasMaxLength(100);

            entity.HasOne(x => x.Declaration)
                .WithMany(x => x.ArchivedDocuments)
                .HasForeignKey(x => x.DeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ClientCompany)
                .WithMany()
                .HasForeignKey(x => x.ClientCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FiscalYear)
                .WithMany()
                .HasForeignKey(x => x.FiscalYearId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DeclarationId);
            entity.HasIndex(x => new { x.ClientCompanyId, x.FiscalYearId });
            entity.HasIndex(x => x.DocumentType);
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
