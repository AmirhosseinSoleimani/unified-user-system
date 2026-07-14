using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Localization.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Localization;

public sealed class ErrorMessageConfiguration : IEntityTypeConfiguration<ErrorMessage>
{
    public void Configure(EntityTypeBuilder<ErrorMessage> builder)
    {
        builder.ToTable("ErrorMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(ErrorMessage.KeyMaxLength);

        builder.Property(x => x.EnglishText)
            .IsRequired()
            .HasMaxLength(ErrorMessage.TextMaxLength);

        builder.Property(x => x.PersianText)
            .IsRequired()
            .HasMaxLength(ErrorMessage.TextMaxLength);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.IsActive);
    }
}
