using AD.Api.Attributes.Services;
using AD.Api.Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Frozen;

namespace AD.Api.Core.Ldap;

public interface IWellKnownService
{
    KeyValuePair<string, DistinguishedName>[] GetAllWellKnownsInDomain(string domain);
}

[DependencyRegistration(typeof(IWellKnownService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class WellKnownService : IWellKnownService
{
    private const string KEY_PREFIX = "WK_";
    private readonly IMemoryCache _cache;
    private readonly ContextLibrary _connections;
    private readonly WellKnownObjectDictionary _dict;

    public WellKnownService(IConnectionService connections, WellKnownObjectDictionary dict, IMemoryCache cache)
    {
        _connections = connections.RegisteredConnections;
        _cache = cache;
        _dict = dict;
    }

    private string CreateKey(string domain)
    {
        domain = _connections[domain].DomainName;

        return string.Create(domain.Length + KEY_PREFIX.Length, domain, (chars, state) =>
        {
            KEY_PREFIX.CopyTo(chars);
            state.CopyTo(chars.Slice(KEY_PREFIX.Length));
        });
    }
    public KeyValuePair<string, DistinguishedName>[] GetAllWellKnownsInDomain(string domain)
    {
        string cacheKey = this.CreateKey(domain);
        if (_cache.TryGetValueOrRemove(cacheKey, out KeyValuePair<string, DistinguishedName>[]? cachedArray))
        {
            return cachedArray;
        }

        ref readonly FrozenDictionary<WellKnownObjectValue, DistinguishedName> all = ref _dict[domain];
        var array = new KeyValuePair<string, DistinguishedName>[all.Count];

        int count = 0;
        foreach (var kvp in all)
        {
            string key = _dict.EnumStrings[kvp.Key];
            array[count++] = new(key, kvp.Value);
        }

        return _cache.Set(cacheKey, array, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
            Priority = CacheItemPriority.Low,
            Size = array.Length * 3L,
        });
    }
}
