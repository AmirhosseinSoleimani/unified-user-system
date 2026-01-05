using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private readonly IClock _clock;
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUser currentUser,
            IClock clock) : base(options) 
        {
            _currentUser = currentUser;
            _clock = clock;
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAudit();
            return base.SaveChangesAsync(cancellationToken);
        }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
        private void ApplyAudit()
        {
            var nowUtc = _clock.Utcnow;
            var actorUserId = _currentUser.UserId;
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is not IAuditableEntity auditable) continue;
                if (entry.State == EntityState.Added)
                {
                    if (auditable.CreatedAt == default)
                        auditable.SetCreated(nowUtc, actorUserId);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedByUserId)).IsModified = false;
                    auditable.Touch(nowUtc, actorUserId);
                }
            }
        }
    }
}
