using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Configurations.Security;

public sealed class IpRuleConfig : AuditableEntityConfig<IpRule, Guid>
{
    public override void Configure(EntityTypeBuilder<IpRule> builder)
    {
        base.Configure(builder);

        builder.ToTable("ip_rules", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.IpAddressOrCidr)
            .HasColumnName("ip_address_or_cidr")
            .HasMaxLength(IpRule.IpAddressOrCidrMaxLength)
            .IsRequired();

        builder.Property(x => x.NormalizedIpAddressOrCidr)
            .HasColumnName("normalized_ip_address_or_cidr")
            .HasMaxLength(IpRule.IpAddressOrCidrMaxLength)
            .IsRequired();

        builder.Property(x => x.RuleType)
            .HasColumnName("rule_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(IpRule.ReasonMaxLength);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.DisabledAt)
            .HasColumnName("disabled_at");

        builder.HasIndex(x => x.NormalizedIpAddressOrCidr);
        builder.HasIndex(x => x.RuleType);
        builder.HasIndex(x => x.IsActive);
    }
}
