//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

//namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
//{
//    public class UserRoleConfig :IEntityTypeConfiguration<UserRole>
//    {
//        public void Configure(EntityTypeBuilder<UserRole> builder)
//        {
//            builder.HasKey(x => new { x.UserId, x.RoleId });

//            builder.HasOne(x => x.User)
//                .WithMany(u => u.UserRoles)
//                .HasForeignKey(x => x.UserId);
//            builder.HasOne(x => x.Role)
//                .WithMany(r => r.UserRoles)
//                .HasForeignKey(x => x.RoleId);
//        }
//    }
//}
