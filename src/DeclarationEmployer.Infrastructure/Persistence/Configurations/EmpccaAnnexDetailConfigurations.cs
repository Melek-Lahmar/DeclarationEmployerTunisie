using System.Text;
using DeclarationEmployer.Domain.Declarations.Empcca;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeclarationEmployer.Infrastructure.Persistence.Configurations;

internal static class EmpccaAnnexDetailConfiguration
{
    public static void Configure<TEntity>(EntityTypeBuilder<TEntity> builder, string tableName)
        where TEntity : class
    {
        builder.ToTable(tableName, "declaration");
        builder.HasKey("LineId");

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetColumnName(ToSnakeCase(property.Name));

            if (property.ClrType == typeof(decimal))
            {
                property.SetPrecision(18);
                property.SetScale(3);
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        var result = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsUpper(character) && index > 0)
            {
                result.Append('_');
            }

            result.Append(char.ToLowerInvariant(character));
        }

        return result.ToString();
    }
}

public sealed class AnnexA1DetailConfiguration : IEntityTypeConfiguration<AnnexA1Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA1Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a1_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA1Detail)
            .HasForeignKey<AnnexA1Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA2DetailConfiguration : IEntityTypeConfiguration<AnnexA2Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA2Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a2_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA2Detail)
            .HasForeignKey<AnnexA2Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA3DetailConfiguration : IEntityTypeConfiguration<AnnexA3Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA3Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a3_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA3Detail)
            .HasForeignKey<AnnexA3Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA4DetailConfiguration : IEntityTypeConfiguration<AnnexA4Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA4Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a4_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA4Detail)
            .HasForeignKey<AnnexA4Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA5DetailConfiguration : IEntityTypeConfiguration<AnnexA5Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA5Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a5_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA5Detail)
            .HasForeignKey<AnnexA5Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA6DetailConfiguration : IEntityTypeConfiguration<AnnexA6Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA6Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a6_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA6Detail)
            .HasForeignKey<AnnexA6Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AnnexA7DetailConfiguration : IEntityTypeConfiguration<AnnexA7Detail>
{
    public void Configure(EntityTypeBuilder<AnnexA7Detail> builder)
    {
        EmpccaAnnexDetailConfiguration.Configure(builder, "annex_a7_details");
        builder.HasOne(x => x.Line).WithOne(x => x.AnnexA7Detail)
            .HasForeignKey<AnnexA7Detail>(x => x.LineId).OnDelete(DeleteBehavior.Cascade);
    }
}
