using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Configuration.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Configuration;

public sealed class ApplicationMetadataConfiguration : IEntityTypeConfiguration<ApplicationMetadata>
{
    public void Configure(EntityTypeBuilder<ApplicationMetadata> builder)
    {
        builder.ToTable("ApplicationMetadata");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LatestWebVersion)
            .IsRequired()
            .HasMaxLength(ApplicationMetadata.VersionMaxLength);

        builder.Property(x => x.LatestMobileVersion)
            .IsRequired()
            .HasMaxLength(ApplicationMetadata.VersionMaxLength);

        builder.Property(x => x.MinimumSupportedWebVersion)
            .HasMaxLength(ApplicationMetadata.VersionMaxLength);

        builder.Property(x => x.MinimumSupportedMobileVersion)
            .HasMaxLength(ApplicationMetadata.VersionMaxLength);

        builder.Property(x => x.WebUpdateUrl)
            .HasMaxLength(ApplicationMetadata.UrlMaxLength);

        builder.Property(x => x.AndroidUpdateUrl)
            .HasMaxLength(ApplicationMetadata.UrlMaxLength);

        builder.Property(x => x.IosUpdateUrl)
            .HasMaxLength(ApplicationMetadata.UrlMaxLength);

        builder.Property(x => x.DefaultLanguage)
            .IsRequired()
            .HasMaxLength(ApplicationMetadata.LanguageMaxLength);

        builder.HasIndex(x => x.IsDeleted);
    }
}
