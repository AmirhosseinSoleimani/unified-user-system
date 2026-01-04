//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

//namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
//{
//    public class RoleConfig : BaseViewEntityTypeConfig<int, Role>
//    {
//        public override void Configure(EntityTypeBuilder<Role> builder)
//        {
//            base.Configure(builder);
//            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
//            builder.Property(x => x.NormalizedName).HasMaxLength(150).IsRequired();
//            builder.HasIndex(x => x.NormalizedName).IsUnique();
//        }

//    }
//}
