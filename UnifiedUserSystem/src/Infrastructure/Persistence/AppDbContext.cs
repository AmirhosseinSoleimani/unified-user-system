using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
        public DbSet<User> Users => Set<User>();
        //public DbSet<Role> Roles => Set<Role>();
        //public DbSet<Operation> Operations => Set<Operation>();
        //public DbSet<UserRole> UserRoles => Set<UserRole>();
        //public DbSet<RoleOperation> RoleOperations => Set<RoleOperation>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<User>(e => 
            {
                e.ToTable("users", "public");
                e.HasKey(e => e.Id);
                e.Property(x => x.Id)
                .HasColumnName("id");

                e.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

                e.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

                e.Property(x => x.Fullname)
                .HasColumnName("full_name")
                .HasMaxLength(255)
                .IsRequired();

                e.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(72)
                .IsRequired();

                e.Property(x => x.CreatedAt)
                .HasColumnName("created_at");

                e.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at");

                e.Property(x => x.Role)
                .HasColumnName("role")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("user");
            });
        }
    }
}
