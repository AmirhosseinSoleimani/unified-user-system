//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

//namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
//{
//    public class RoleOperationConfig : IEntityTypeConfiguration<RoleOperation>
//    {
//        public void Configure(EntityTypeBuilder<RoleOperation> builder)
//        {
//            builder.HasKey(x => new { x.RoleId, x.OperationId });

//            builder.HasOne(x => x.Role)
//                .WithMany(r => r.RoleOperations)
//                .HasForeignKey(x => x.OperationId);

//            builder.HasOne(x => x.Operation)
//                .WithMany(o => o.RoleOperations)
//                .HasForeignKey(x => x.OperationId);
//        }
//    }
//}
