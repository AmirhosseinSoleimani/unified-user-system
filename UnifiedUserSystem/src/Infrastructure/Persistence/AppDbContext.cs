using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Auditing.Entities;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private readonly IClock _clock;
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUser currentUser,
            IClock clock
            ) : base(options) 
        {
            _currentUser = currentUser;
            _clock = clock;
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Operation> Operation => Set<Operation>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RoleOperation> RoleOperations => Set<RoleOperation>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var prop = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                    var body = Expression.Equal(prop, Expression.Constant(false));
                    var lambda = Expression.Lambda(body, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(x => x.EntityName).IsRequired().HasMaxLength(128);
                entity.Property(x => x.EntityId).IsRequired().HasMaxLength(128);
                entity.Property(x => x.Action).IsRequired().HasMaxLength(128);
                entity.Property(x => x.OldValues);
                entity.Property(x => x.NewValues);
            });
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAudit();
            return base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            ApplyAudit();
            return base.SaveChanges();
        }
        private void ApplyAudit()
        {
            var nowUtc = _clock.Utcnow;
            var actorUserId = _currentUser.UserId;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Deleted)
                    continue;

                if (entry.Entity is ISoftDeletable softDeletable)
                {
                    entry.State = EntityState.Modified;

                    softDeletable.SoftDelete(nowUtc, actorUserId);
                }
            }
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreated(nowUtc, actorUserId);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Touch(nowUtc, actorUserId);
                }
            }
        }
    }
}
