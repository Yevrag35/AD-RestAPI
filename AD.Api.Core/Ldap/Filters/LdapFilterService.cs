using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Enums;
using AD.Api.Ldap.Enums;
using AD.Api.Strings.Spans;

namespace AD.Api.Core.Ldap
{
    public interface ILdapFilterService
    {
        string GetFilter(FilteredRequestType types, bool addEnclosure);
    }

    [DependencyRegistration(typeof(ILdapFilterService), Lifetime = ServiceLifetime.Singleton)]
    internal sealed class LdapFilterService : ILdapFilterService
    {
        //private const string COMPUTER_FILTER = "(&(objectClass=computer)(objectCategory=computer))";
        //private const string CONTACT_FILTER = "(&(objectClass=contact)(objectCategory=contact))";
        //private const string CONTAINER_FILTER = "(&(objectClass=container)(objectCategory=container))";
        //private const string GROUP_FILTER = "(&(objectClass=group)(objectCategory=group))";
        //private const string MSA_FILTER = "(&(objectClass=msDS-ManagedServiceAccount)(objectCategory=msDS-ManagedServiceAccount))";
        //private const string ORGANIZATIONAL_UNIT_FILTER = "(&(objectClass=organizationalUnit)(objectCategory=organizationalUnit))";
        //private const string USER_FILTER = "(&(objectClass=user)(objectCategory=person))";

        public IEnumValues<FilteredRequestType, BackendValueAttribute, string> FilterValues { get; }
        public IEnumStrings<FilteredRequestType> RequestTypes { get; }

        public LdapFilterService(IEnumValues<FilteredRequestType, BackendValueAttribute, string> filterValues)
        {
            this.FilterValues = filterValues;
            this.RequestTypes = filterValues.EnumStrings;
        }

        public string GetFilter(FilteredRequestType types, bool addEnclosure)
        {
            SpanStringBuilder builder = new(stackalloc char[256]);
            if (addEnclosure)
            {
                builder = builder.Append(['(', '&']);
            }

            int count = this.GetEnumerationNumber(types, ref builder);
            if (count <= 0)
            {
                builder.Dispose();
                return string.Empty;
            }

            if (addEnclosure)
            {
                builder = builder.Append(')');
            }

            string s = builder.Build();
            return s;
        }

        private int GetEnumerationNumber(FilteredRequestType value, ref SpanStringBuilder builder)
        {
            if (this.FilterValues.TryGetValue(value, out string? filter))
            {
                builder = builder.Append(filter);
                return 1;
            }

            // Is a combination of flags
            FlagEnumerator<FilteredRequestType> enumerator = new(value);

            while (enumerator.MoveNext())
            {
                if (this.FilterValues.TryGetValue(enumerator.Current, out string? filterValue))
                {
                    builder = builder.Append(filterValue);
                }
            }

            return enumerator.Count;
        }
    }
}

