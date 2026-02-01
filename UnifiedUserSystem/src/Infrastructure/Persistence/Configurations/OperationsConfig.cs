using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public class OperationsConfig : AuditableEntityConfig<Operation, Guid>
    {
        public override void Configure(EntityTypeBuilder<Operation> builder)
        {
            base.Configure(builder);

            builder.ToTable("operations", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .HasColumnName("key")
                .HasMaxLength(Operation.KeyMaxLength)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(Operation.TitleMaxLength)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.HasIndex(x => x.Key).IsUnique();
        }
    }
}
