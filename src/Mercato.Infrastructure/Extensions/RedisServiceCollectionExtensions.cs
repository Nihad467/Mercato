using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Mercato.Infrastructure.Extensions;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddRedisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(
            configuration.GetSection("Redis"));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisOptions = configuration
                .GetSection("Redis")
                .Get<RedisOptions>();

            if (redisOptions is null || string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
                throw new InvalidOperationException("Redis connection string is missing.");

            return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }
}