using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Enums;
using AD.Api.Ldap.Enums;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Ldap.Services
{
    public interface ILdapFilterService
    {

    }

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

        //public string AddToFilter(ReadOnlySpan<char> filter, FilteredRequestType types)
        //{
        //}
        //public string GetFilter(FilteredRequestType types)
        //{
        //    if (this.RequestTypes.ContainsEnum(types))
        //    {

        //    }
        //}

        private int GetEnumerationLength(FilteredRequestType value, out int numberOfFlagsSet)
        {
            if (this.RequestTypes.TryGetName(value, out string? name))
            {
                numberOfFlagsSet = 1;
                return name.Length;
            }

            // Is a combination of flags
            FlagEnumerator<FilteredRequestType> enumerator = new(value);
            int length = 0;

            while (enumerator.MoveNext())
            {
                if (this.FilterValues.TryGetValue(enumerator.Current, out string? filterValue))
                {
                    length += filterValue.Length;
                }
            }

            numberOfFlagsSet = enumerator.Count;
            return length;
        }
    }
}

