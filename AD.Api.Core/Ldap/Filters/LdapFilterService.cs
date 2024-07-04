using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Security;
using AD.Api.Enums;

namespace AD.Api.Core.Ldap.Filters
{
    public interface ILdapFilterService
    {
        string AddToFilter(scoped ReadOnlySpan<char> filter, FilteredRequestType types, bool addEnclosure);
        string GetFilter(FilteredRequestType types, bool addEnclosure);
        string GetFilter(SidString sidString, FilteredRequestType types);
    }

    [DependencyRegistration(typeof(ILdapFilterService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class LdapFilterService : ILdapFilterService
    {
        public IEnumValues<FilteredRequestType, BackendValueAttribute, string> FilterValues { get; }
        public IEnumStrings<FilteredRequestType> RequestTypes { get; }

        public LdapFilterService(IEnumValues<FilteredRequestType, BackendValueAttribute, string> filterValues)
        {
            this.FilterValues = filterValues;
            this.RequestTypes = filterValues.EnumStrings;
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

        public string GetFilter(SidString sidString, FilteredRequestType types)
        {
            FilterSpanWriter writer = new(stackalloc char[256]);
            writer = writer.And();

            _ = this.GetEnumerationNumber(types, ref writer);
            writer.Equal("objectSid"u8, sidString, SidString.MaxSidStringLength, SidString.LdapFormat);
            writer.EndAll();

            return writer.Build();
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
    }
}

