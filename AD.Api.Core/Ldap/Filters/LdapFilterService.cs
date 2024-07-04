using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Extensions;
using AD.Api.Core.Security;
using AD.Api.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace AD.Api.Core.Ldap.Filters
{
    public interface ILdapFilterService
    {
        string AddToFilter(scoped ReadOnlySpan<char> filter, FilteredRequestType types, bool addEnclosure);
        string GetFilter(FilteredRequestType types, bool addEnclosure);
        string GetFilter(SidString sidString, FilteredRequestType types);
        string GetFilter(SidString sidString, FilteredRequestType types, bool noCache);
    }

    [DependencyRegistration(typeof(ILdapFilterService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class LdapFilterService : ILdapFilterService
    {
        private readonly IMemoryCache _cache;

        public IEnumValues<FilteredRequestType, BackendValueAttribute, string> FilterValues { get; }
        public IEnumStrings<FilteredRequestType> RequestTypes { get; }

        public LdapFilterService(IEnumValues<FilteredRequestType, BackendValueAttribute, string> filterValues, IMemoryCache cache)
        {
            this.FilterValues = filterValues;
            this.RequestTypes = filterValues.EnumStrings;
            _cache = cache;
        }

        public string AddToFilter(scoped ReadOnlySpan<char> filter, FilteredRequestType types, bool addEnclosure)
        {
            if (filter.IsWhiteSpace())
            {
                return this.GetFilter(types, addEnclosure);
            }

            FilterSpanWriter writer = new(filter.Length + 130);

            if (addEnclosure)
            {
                writer = writer.And();
            }

            int count = this.GetEnumerationNumber(types, ref writer);
            if (count <= 0)
            {
                writer.Dispose();
                return filter.ToString();
            }

            writer = writer.WriteRaw(filter);

            writer = writer.EndAll();

            string s = writer.Build();
            return s;
        }
        public string GetFilter(FilteredRequestType types, bool addEnclosure)
        {
            FilterSpanWriter writer = new(stackalloc char[256]);
            if (addEnclosure)
            {
                writer = writer.And();
            }

            int count = this.GetEnumerationNumber(types, ref writer);
            if (count <= 0)
            {
                writer.Dispose();
                return string.Empty;
            }

            writer = writer.EndAll();

            string s = writer.Build();
            return s;
        }

        [DebuggerStepThrough]
        public string GetFilter(SidString sidString, FilteredRequestType types)
        {
            return this.GetFilter(sidString, types, noCache: false);
        }
        public string GetFilter(SidString sidString, FilteredRequestType types, bool noCache)
        {
            if (!noCache && this.TryGetFilterFromCache(sidString, out string? filter))
            {
                return filter;
            }

            FilterSpanWriter writer = new(stackalloc char[256]);
            writer = writer.And();

            _ = this.GetEnumerationNumber(types, ref writer);
            writer.Equal("objectSid"u8, sidString, sidString.LdapStringLength, SidString.LdapFormat);
            writer.EndAll();

            filter = writer.Build();

            return !noCache
                ? this.AddFilterToCache(sidString, filter)
                : filter;
        }

        [DebuggerStepThrough]
        private string AddFilterToCache(SidString sidString, string filter)
        {
            return _cache.Set(sidString, filter, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Priority = CacheItemPriority.Low,
                Size = Math.Max((long)Math.Floor(filter.Length / 3d), 3L),
            });
        }
        private int GetEnumerationNumber(FilteredRequestType value, ref FilterSpanWriter writer)
        {
            if (this.FilterValues.TryGetValue(value, out string? filter))
            {
                writer = writer.WriteRaw(filter);
                return 1;
            }

            // Is a combination of flags
            FlagEnumerator<FilteredRequestType> enumerator = new(value);

            while (enumerator.MoveNext())
            {
                if (this.FilterValues.TryGetValue(enumerator.Current, out string? filterValue))
                {
                    writer = writer.WriteRaw(filterValue);
                }
            }

            return enumerator.Count;
        }
        [DebuggerStepThrough]
        private bool TryGetFilterFromCache(SidString sidString, [NotNullWhen(true)] out string? filter)
        {
            return _cache.TryGetValueOrRemove(sidString, out filter);
        }
    }
}

