using AD.Api.Attributes.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace AD.Api.Core.Authentication.Jwt;

[DependencyRegistration(Lifetime = ServiceLifetime.Singleton)]
internal sealed class JwtCache
{
    private readonly ConcurrentDictionary<string, TokenKey> _keys;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _lifetime;
    private readonly TimeSpan _skew;
    internal TimeProvider Clock { get; }
    internal JwtCache(IMemoryCache cache, TimeProvider clock, CustomJwtSettings settings)
    {
        this.Clock = clock;
        _cache = cache;
        _lifetime = settings.TokenLifetime;
        _skew = settings.ExpirationSkew;
        _keys = new(Environment.ProcessorCount, 3, StringComparer.Ordinal);
    }

    public DateTimeOffset GetExpirationStamp()
    {
        return this.Clock.GetUtcNow().Add(_lifetime);
    }
    public bool IsExpired(BearerToken token)
    {
        var expiration = token.Expires - this.Clock.GetUtcNow();
    }
    public bool TryGetToken(string key, [NotNullWhen(true)] out BearerToken? token)
    {
        if (!_keys.TryRemove(key, out TokenKey? tokenKey))
        {
            tokenKey = new(key);
        }

        if (_cache.TryGetValue(tokenKey, out token))
        {

        }
    }
}
