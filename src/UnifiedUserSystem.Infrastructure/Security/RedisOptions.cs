
namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "UnifiedUserSystem:";
}
