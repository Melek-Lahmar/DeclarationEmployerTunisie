namespace DeclarationEmployer.Infrastructure.Configuration;

public sealed class BackupOptions
{
    public string? PgDumpPath { get; set; }

    public string? PgRestorePath { get; set; }

    public string Directory { get; set; } = "C:/DET2025_DEV/storage/backups";

    public int RetentionDays { get; set; } = 30;
}
