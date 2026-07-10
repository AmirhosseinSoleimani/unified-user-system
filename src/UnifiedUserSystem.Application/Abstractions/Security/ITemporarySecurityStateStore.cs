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

public sealed class SecurityStateUnavailableException : Exception
{
    public SecurityStateUnavailableException(string message)
        : base(message)
    {
    }

    public SecurityStateUnavailableException(string message, Exception innerException)
       : base(message, innerException)
    {
    }
}