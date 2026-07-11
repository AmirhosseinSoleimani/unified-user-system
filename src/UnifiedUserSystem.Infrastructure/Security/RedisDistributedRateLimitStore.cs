
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class RedisDistributedRateLimitStore : IDistributedRateLimitStore
{
    private readonly IConnectionMultiplexer _connection;
    private readonly RedisOptions _options;

    public RedisDistributedRateLimitStore(
        IConnectionMultiplexer connection,
        IOptions<RedisOptions> options)
    {
        _connection = connection;
        _options = options.Value;
    }

    public async Task<DistributedRateLimitLeaseResult> TryAcquireAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        TimeSpan? cooldown,
        CancellationToken cancellationToken = default)
    {
        var counterKey = ToRedisKey(key);
        var cooldownKey = ToRedisKey($"{key}:cooldown");

        try
        {
            var cooldownTtl = await Database.KeyTimeToLiveAsync(cooldownKey);
            if (cooldownTtl.HasValue)
            {
                return DistributedRateLimitLeaseResult.Rejected(
                    permitLimit + 1,
                    cooldownTtl,
                    "Rate limit cooldown is active.");
            }

            const string script = """
                local current = redis.call('INCR', KEYS[1])
                if current == 1 then
                    redis.call('PEXPIRE', KEYS[1], ARGV[1])
                end
                local ttl = redis.call('PTTL', KEYS[1])
                return { current, ttl }
                """;

            var result = (RedisResult[])(await Database.ScriptEvaluateAsync(
                script,
                new RedisKey[] { counterKey },
                new RedisValue[] { (long)window.TotalMilliseconds }));

            var count = (long)result[0];
            var ttlMilliseconds = (long)result[1];
            var retryAfter = ttlMilliseconds > 0
                ? TimeSpan.FromMilliseconds(ttlMilliseconds)
                : window;

            if (count <= permitLimit)
                return DistributedRateLimitLeaseResult.Acquired(count, retryAfter);

            if (cooldown.HasValue && cooldown.Value > TimeSpan.Zero)
            {
                await Database.StringSetAsync(cooldownKey, "1", cooldown.Value);
                retryAfter = cooldown.Value;
            }

            return DistributedRateLimitLeaseResult.Rejected(
                count,
                retryAfter,
                "Too many requests.");
        }
        catch (RedisException ex)
        {
            return DistributedRateLimitLeaseResult.Unavailable(ex.Message);
        }
    }

    public async Task ResetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.KeyDeleteAsync(new RedisKey[]
            {
                ToRedisKey(key),
                ToRedisKey($"{key}:cooldown")
            });
        }
        catch (RedisException)
        {
            // Reset is best-effort; callers still enforce fail-safe decisions on acquire/check.
        }
    }

    private IDatabase Database => _connection.GetDatabase();

    private RedisKey ToRedisKey(string key)
        => $"{_options.InstanceName}{key}";
}