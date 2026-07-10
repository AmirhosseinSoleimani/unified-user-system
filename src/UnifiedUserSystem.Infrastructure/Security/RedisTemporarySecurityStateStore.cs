using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class RedisTemporarySecurityStateStore : ITemporarySecurityStateStore
{
    private readonly IConnectionMultiplexer _connection;
    private readonly RedisOptions _options;

    public RedisTemporarySecurityStateStore(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await Database.StringGetAsync(ToRedisKey(key));
            return value.HasValue ? value.ToString() : null;
        }
        catch (RedisException ex)
        {
            throw new SecurityStateUnavailableException("Redis security state is unavailable.", ex);
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            await Database.StringSetAsync(ToRedisKey(key), value, ttl);
        }
        catch (RedisException ex)
        {
            throw new SecurityStateUnavailableException("Redis security state is unavailable.", ex);
        }
    }

    public async Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        const string script = """
            local current = redis.call('INCR', KEYS[1])
            if current == 1 then
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
            end
            return current
            """;

        try
        {
            var result = await Database.ScriptEvaluateAsync(
                script,
                new RedisKey[] { ToRedisKey(key) },
                new RedisValue[] { (long)ttl.TotalMilliseconds });

            return (long)result;
        }
        catch (RedisException ex)
        {
            throw new SecurityStateUnavailableException("Redis security state is unavailable.", ex);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await Database.KeyDeleteAsync(ToRedisKey(key));
        }
        catch (RedisException ex)
        {
            throw new SecurityStateUnavailableException("Redis security state is unavailable.", ex);
        }
    }

    private StackExchange.Redis.IDatabase Database => _connection.GetDatabase();

    private RedisKey ToRedisKey(string key)
        => $"{_options.InstanceName}{key}";
}
