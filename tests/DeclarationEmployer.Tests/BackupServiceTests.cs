using DeclarationEmployer.Application.Auth;
using DeclarationEmployer.Application.Common;
using DeclarationEmployer.Contracts.Backup;
using DeclarationEmployer.Domain.Backup;
using DeclarationEmployer.Infrastructure.Configuration;
using DeclarationEmployer.Infrastructure.Persistence;
using DeclarationEmployer.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DeclarationEmployer.Tests;

public sealed class BackupServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsMissingPgDumpPath()
    {
        await using var db = CreateDbContext();
        var service = new BackupService(
            db,
            new FakeCurrentUserService(),
            new FileHashService(),
            Options.Create(new BackupOptions { PgDumpPath = "C:/missing/pg_dump.exe" }));

        var act = () => service.CreateAsync(new CreateBackupRequest());

        await act.Should().ThrowAsync<ApplicationConflictException>();
    }

    [Fact]
    public async Task VerifyAsync_MarksRecordVerifiedWhenHashMatches()
    {
        using var fixture = CreateFixture();
        var filePath = Path.Combine(fixture.RootPath, "backup.dump");
        await File.WriteAllTextAsync(filePath, "backup-content");
        var hash = await new FileHashService().ComputeSha256Async(filePath);
        var record = new BackupRecord
        {
            Id = Guid.NewGuid(),
            FileName = "backup.dump",
            StoredPath = filePath,
            Sha256Hash = hash,
            SizeBytes = new FileInfo(filePath).Length,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = BackupRecordStatus.Created
        };
        fixture.Db.BackupRecords.Add(record);
        await fixture.Db.SaveChangesAsync();

        var result = await fixture.Service.VerifyAsync(record.Id);

        result.Status.Should().Be(BackupRecordStatus.Verified.ToString());
        fixture.Db.BackupEvents.Should().Contain(x => x.Action == "BACKUP_VERIFIED");
    }

    private static BackupFixture CreateFixture()
    {
        var db = CreateDbContext();
        var rootPath = Path.Combine(Path.GetTempPath(), "DeclarationEmployerTests", "Backup", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);
        var service = new BackupService(
            db,
            new FakeCurrentUserService(),
            new FileHashService(),
            Options.Create(new BackupOptions { Directory = rootPath }));

        return new BackupFixture(db, service, rootPath);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record BackupFixture(ApplicationDbContext Db, BackupService Service, string RootPath) : IDisposable
    {
        public void Dispose()
        {
            Db.Dispose();
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;

        public string? UserName => null;

        public string? Role => null;

        public bool IsAuthenticated => false;
    }
}
