namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface ITemporarySecurityStateStore
{
    Task<string?> GetStringAsync(string key, CancellationToken ct = default);

    Task SetStringAsync(
        string key,
        string value,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task<long> IncrementAsync(
        string key,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task RemoveAsync(string key, CancellationToken ct = default);
}