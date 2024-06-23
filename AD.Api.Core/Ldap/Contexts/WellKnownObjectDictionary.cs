using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Collections.Enumerators;
using AD.Api.Ldap.Enums;
using System.Collections.Frozen;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Singleton)]
    public sealed class WellKnownObjectDictionary
    {
        private const string ATT_NAME = "wellKnownObjects";
        private const string OTHER_NAME = "otherWellKnownObjects";
        private const string FILTER = "(objectClass=domainDNS)";
        private readonly IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> _values;

        private readonly FrozenDictionary<string, FrozenDictionary<WellKnownObjectValue, string>> _dictionary;

        public ref readonly FrozenDictionary<WellKnownObjectValue, string> this[string? key] => ref _dictionary[key ?? string.Empty];

        public WellKnownObjectDictionary(IConnectionService connections, IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> enumValues)
        {
            _values = enumValues;
            string[] atts = [ATT_NAME, OTHER_NAME];
            Dictionary<string, FrozenDictionary<WellKnownObjectValue, string>> wkByDomain = new(connections.RegisteredConnections.Count, StringComparer.OrdinalIgnoreCase);

            //foreach (IGrouping<string, ConnectionContext> grouping in connections.RegisteredConnections.GroupBy(x => x))
            foreach (var grouping in connections.RegisteredConnections.Keys.GroupBy(x => connections.RegisteredConnections[x]))
            {
                Dictionary<WellKnownObjectValue, string> dict = new(_values.EnumCount);
                FindDomainWellKnownLocations(dict, grouping.Key, enumValues, atts);
                var frozen = dict.ToFrozenDictionary();
                foreach (string key in grouping)
                {
                    wkByDomain.Add(key, frozen);
                }
            }

            _dictionary = wkByDomain.ToFrozenDictionary(wkByDomain.Comparer);
        }

        public bool TryGetValue(string? domainKey, WellKnownObjectValue type, [NotNullWhen(true)] out string? location)
        {
            domainKey ??= string.Empty;
            if (_dictionary[domainKey].TryGetValue(type, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                location = value;
                return true;
            }
            else
            {
                location = null;
                return false;
            }
        }

        private static void FindDomainWellKnownLocations(Dictionary<WellKnownObjectValue, string> dict, ConnectionContext context, IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> enumValues, string[] attributes)
        {
            using var connection = context.CreateConnection();
            List<string> locations = GetLocationValues(connection, context, attributes);
            if (locations.Count <= 0)
            {
                return;
            }

            foreach (WellKnownObjectValue wk in enumValues.EnumStrings.Values)
            {
                string guid = enumValues.GetValue(wk, string.Empty);
                string location = MatchLocationToGuid(guid, locations);

                dict.TryAdd(wk, location);
            }
        }
        private static string[] GetAttributeValue(string attributeName, SearchResultEntry entries)
        {
            return entries.Attributes[attributeName].GetValues(typeof(string)) as string[] ?? [];
        }
        private static List<string> GetLocationValues(LdapConnection connection, ConnectionContext context, string[] attributes)
        {
            List<string> list = new(15);

            SearchRequest request = new(context.DefaultNamingContext, FILTER, SearchScope.Base, attributes);
            SearchResponse response = (SearchResponse)connection.SendRequest(request);
            SearchResultEntry entry = response.Entries[0];

            foreach (string attribute in attributes)
            {
                string[] values = GetAttributeValue(attribute, entry);
                list.AddRange(values);
            }

            return list;
        }
        private static string MatchLocationToGuid(string guid, List<string> locations)
        {
            ArrayRefEnumerator<string> enumerator = new(locations);
            bool flag = false;
            int index = -1;

            while (enumerator.MoveNext(in flag, ref index))
            {
                flag = enumerator.Current.AsSpan().Contains(guid, StringComparison.OrdinalIgnoreCase);
            }

            if (!flag || index < 0)
            {
                return string.Empty;
            }

            ReadOnlySpan<char> location = locations[index].AsSpan();
            index = location.LastIndexOf(':') + 1;
            return index > 0 && index < location.Length
                ? location.Slice(index).Trim().ToString()
                : string.Empty;
        }
    }
}

