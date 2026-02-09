using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Infrastructure.Persistence.Configurations;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public class RoleOperationConfig : AuditableEntityConfig<RoleOperation, Guid>
    {
        public override void Configure(EntityTypeBuilder<RoleOperation> builder)
        {
            base.Configure(builder);

            builder.ToTable("role_operations", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(x => x.OperationId)
                .HasColumnName("operation_id")
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(r => r.RoleOperations)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Operation)
                .WithMany(o => o.RoleOperations)
                .HasForeignKey(x => x.OperationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.RoleId, x.OperationId }).IsUnique();
        }
    }
}
