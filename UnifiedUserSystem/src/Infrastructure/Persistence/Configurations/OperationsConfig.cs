//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

//namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
//{
//    public class OperationsConfig : BaseViewEntityTypeConfig<int, Operation>
//    {
//        public override void Configure(EntityTypeBuilder<Operation> builder)
//        {
//            base.Configure(builder);
//            builder.Property(x => x.Key).HasMaxLength(150).IsRequired();
//            builder.HasIndex(x => x.Key).IsUnique();
//            builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
//        }
//    }
//}
