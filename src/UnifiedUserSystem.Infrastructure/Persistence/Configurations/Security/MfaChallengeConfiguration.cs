
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Security;

public sealed class MfaChallengeConfiguration : IEntityTypeConfiguration<MfaChallenge>
{
    public void Configure(EntityTypeBuilder<MfaChallenge> builder)
    {
        builder.ToTable("mfa_challenges");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OtpHash)
            .HasColumnName("otp_hash")
            .HasMaxLength(MfaChallenge.OtpHashMaxLength)
            .IsRequired();

        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(x => x.MaxAttempts).HasColumnName("max_attempts").IsRequired();
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired();
        builder.Property(x => x.IsUsed).HasColumnName("is_used").IsRequired();
        builder.Property(x => x.VerifiedAt).HasColumnName("verified_at");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId).HasDatabaseName("ix_mfa_challenges_user_id");
        builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("ix_mfa_challenges_expires_at");
        builder.HasIndex(x => x.IsUsed).HasDatabaseName("ix_mfa_challenges_is_used");
        builder.HasIndex(x => new { x.UserId, x.Channel, x.IsUsed })
            .HasDatabaseName("ix_mfa_challenges_user_channel_is_used");
    }
}
