using System.Text.Json;
using Mercato.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Mercato.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await _database.StringGetAsync(key);

        if (cachedValue.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<T>(
            cachedValue!,
            JsonSerializerOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(
            value,
            JsonSerializerOptions);

        await _database.StringSetAsync(
            key,
            serializedValue,
            expiration);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var endpoints = _connectionMultiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);

            var keys = server.Keys(
                pattern: $"{prefix}*").ToArray();

            if (keys.Length == 0)
                continue;

            await _database.KeyDeleteAsync(keys);
        }
    }
}