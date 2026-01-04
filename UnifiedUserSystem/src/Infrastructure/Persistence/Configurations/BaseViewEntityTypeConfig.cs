using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Common;
namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence.Configurations
{
    public class BaseViewEntityTypeConfig<TKey, TEntity> : IEntityTypeConfiguration<TEntity> 
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.ID);
            builder.Property(e => e.ID).IsRequired();

            if (typeof(ILoggableEntityNameAndID).IsAssignableFrom(typeof(TEntity))) 
            {
                builder.OwnsOne<EntityNameAndIDLogData>(
                    navigationName: "LogData",
                    owned =>
                    {
                        owned.Property(e => e.InsertUserId).HasColumnName("INSERT_USERID").IsRequired(false);
                        owned.Property(e => e.InsertUserName).HasColumnName("INSERT_USERNAME").IsRequired(false);
                        owned.Property(e => e.InsertDateTime).HasColumnName("INSERT_DATE").IsRequired(false);
                        owned.Property(e => e.UpdateUserId).HasColumnName("UPDATE_USERID");
                        owned.Property(e => e.UpdateUserName).HasColumnName("UPDATE_USERNAME");
                        owned.Property(e => e.UpdateDateTime).HasColumnName("UPDATE_DATE");
                        owned.Ignore(e => e.ObjectState);
                        owned.Ignore(e => e.ID);
                    }
                   );
            }
        }

    }
}
