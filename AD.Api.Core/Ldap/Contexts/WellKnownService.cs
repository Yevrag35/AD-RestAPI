using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Frozen;

namespace AD.Api.Core.Ldap;

public interface IWellKnownService
{
    KeyValuePair<string, DistinguishedName>[] GetAllWellKnownsInDomain(string domain);
    DistinguishedName GetValueByKey(string domain, WellKnownObjectValue key);
    bool TryGetValueByKey(string domain, WellKnownObjectValue key, out DistinguishedName location);
    bool TryGetValueByRequestType(string? domain, FilteredRequestType requestType, out DistinguishedName location);
}

[DependencyRegistration(typeof(IWellKnownService), Lifetime = ServiceLifetime.Singleton)]
internal sealed class WellKnownService : IWellKnownService
{
    private const string KEY_PREFIX = "WK_";
    private readonly IMemoryCache _cache;
    private readonly ContextLibrary _connections;
    private readonly WellKnownObjectDictionary _dict;

    public WellKnownService(IConnectionService connections, IMemoryCache cache, IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> enumValues)
    {
        _connections = connections.RegisteredConnections;
        _cache = cache;
        _dict = new(connections, enumValues);
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
    public DistinguishedName GetValueByKey(string domain, WellKnownObjectValue key)
    {
        return _dict.TryGetValue(domain, key, out DistinguishedName location)
            ? location
            : DistinguishedName.Empty;
    }
    public bool TryGetValueByKey(string domain, WellKnownObjectValue key, out DistinguishedName location)
    {
        return _dict.TryGetValue(domain, key, out location);
    }
    public bool TryGetValueByRequestType(string? domain, FilteredRequestType requestType, out DistinguishedName location)
    {
        return _dict.TryGetValue(domain, requestType, out location);
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
}
