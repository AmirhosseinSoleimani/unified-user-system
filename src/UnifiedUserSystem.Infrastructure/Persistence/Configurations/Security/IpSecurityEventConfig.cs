
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Security;

public sealed class IpSecurityEventConfig : IEntityTypeConfiguration<IpSecurityEvent>
{
    public void Configure(EntityTypeBuilder<IpSecurityEvent> builder)
    {
        builder.ToTable("ip_security_events", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(IpSecurityEvent.IpAddressMaxLength)
            .IsRequired();

        builder.Property(x => x.NormalizedIpAddress)
            .HasColumnName("normalized_ip_address")
            .HasMaxLength(IpSecurityEvent.IpAddressMaxLength)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Path)
            .HasColumnName("path")
            .HasMaxLength(IpSecurityEvent.PathMaxLength);

        builder.Property(x => x.HttpMethod)
            .HasColumnName("http_method")
            .HasMaxLength(IpSecurityEvent.HttpMethodMaxLength);

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.RuleId)
            .HasColumnName("rule_id");

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(IpSecurityEvent.ReasonMaxLength);

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.HasIndex(x => x.NormalizedIpAddress);
        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.RuleId);
    }
}
