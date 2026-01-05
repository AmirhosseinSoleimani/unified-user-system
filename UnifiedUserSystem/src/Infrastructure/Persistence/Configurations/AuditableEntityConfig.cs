using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations
{
    public abstract class AuditableEntityConfig<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IAuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            builder.Property(x => x.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired(false);

            builder.Property(x => x.UpdatedByUserId)
                .HasColumnName("updated_by_user_id")
                .IsRequired(false);
        }
    }
}
