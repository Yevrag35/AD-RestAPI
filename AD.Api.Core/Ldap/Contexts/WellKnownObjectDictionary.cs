using AD.Api.Attributes;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Enums;
using System.Collections.Frozen;

namespace AD.Api.Core.Ldap;

internal sealed class WellKnownObjectDictionary
{
	private const string ATT_NAME = "wellKnownObjects";
	private const string OTHER_NAME = "otherWellKnownObjects";
	private const string FILTER = "(objectClass=domainDNS)";
	private readonly IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> _values;

	private readonly FrozenDictionary<string, FrozenDictionary<WellKnownObjectValue, DistinguishedName>> _dictionary;

	public IEnumStrings<WellKnownObjectValue> EnumStrings => _values.EnumStrings;

	public ref readonly FrozenDictionary<WellKnownObjectValue, DistinguishedName> this[string? key] => ref _dictionary[key ?? string.Empty];

	public WellKnownObjectDictionary(IConnectionService connections, IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> enumValues)
	{
		_values = enumValues;
		string[] atts = [ATT_NAME, OTHER_NAME];

		Dictionary<string, FrozenDictionary<WellKnownObjectValue, DistinguishedName>> wkByDomain = new(connections.RegisteredConnections.Count, StringComparer.OrdinalIgnoreCase);

		foreach (var grouping in connections.RegisteredConnections.Keys.GroupBy(x => connections.RegisteredConnections[x]))
		{
			Dictionary<WellKnownObjectValue, DistinguishedName> dict = new(_values.EnumCount);
			FindDomainWellKnownLocations(dict, grouping.Key, enumValues, atts);
			var frozen = dict.ToFrozenDictionary();
			foreach (string key in grouping)
			{
				wkByDomain.Add(key, frozen);
			}
		}

		_dictionary = wkByDomain.ToFrozenDictionary(wkByDomain.Comparer);
	}

	internal bool TryGetValue(string? domainKey, FilteredRequestType requestType, out DistinguishedName location)
	{
		if (!HasWellKnownPath(requestType, out WellKnownObjectValue wkValue))
		{
			location = DistinguishedName.Empty;
			return false;
		}

		return this.TryGetValue(domainKey, wellKnownType: wkValue, out location);
	}
	internal bool TryGetValue(string? domainKey, WellKnownObjectValue wellKnownType, out DistinguishedName location)
	{
		domainKey ??= string.Empty;
		if (_dictionary[domainKey].TryGetValue(wellKnownType, out DistinguishedName value) && !value.IsEmpty)
		{
			location = value;
			return true;
		}
		else
		{
			location = DistinguishedName.Empty;
			return false;
		}
	}

	/// <summary>
	/// Determines if the specified <see cref="FilteredRequestType"/> would normally have a well-known path in 
	/// the directory.
	/// </summary>
	/// <param name="requestType">The request type to check.</param>
	/// <param name="value">
	/// When this method returns, contains the well-known object value for the specified 
	/// <paramref name="requestType"/> if it exists; otherwise, <see cref="WellKnownObjectValue.None"/>.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if the specified <paramref name="requestType"/> has a well-known path; otherwise, 
	/// <see langword="false"/>.
	/// </returns>
	internal static bool HasWellKnownPath(FilteredRequestType requestType, out WellKnownObjectValue value)
	{
		switch (requestType)
		{
			case FilteredRequestType.Any:
			case FilteredRequestType.Container:
			case FilteredRequestType.OrganizationalUnit:
				goto default;

			case FilteredRequestType.User:
			case FilteredRequestType.Group:
			case FilteredRequestType.Contact:
				value = WellKnownObjectValue.Users;
				return true;

			case FilteredRequestType.Computer:
				value = WellKnownObjectValue.Computers;
				return true;

			case FilteredRequestType.ManagedServiceAccount:
				value = WellKnownObjectValue.ManagedServiceAccounts;
				return true;

			default:
				value = WellKnownObjectValue.None;
				return false;
		}
	}

	private static void FindDomainWellKnownLocations(Dictionary<WellKnownObjectValue, DistinguishedName> dict, ConnectionContext context, IEnumValues<WellKnownObjectValue, BackendValueAttribute, string> enumValues, string[] attributes)
	{
		using var connection = context.CreateConnection();
		List<string> locations = GetLocationValues(connection, context, attributes);
		if (locations.Count <= 0)
		{
			return;
		}

		foreach (WellKnownObjectValue wk in enumValues.EnumStrings.Values.Where(x => WellKnownObjectValue.None != x))
		{
			string guid = enumValues.GetValueOrDefault(wk, string.Empty);
			DistinguishedName locationDn = DistinguishedName.Parse(MatchLocationToGuid(guid, locations));

			dict.TryAdd(wk, locationDn);
		}
	}
	private static string[] GetAttributeValue(string attributeName, SearchResultEntry entries)
	{
		return entries.Attributes[attributeName].GetStringArray();
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
	private static string MatchLocationToGuid(ReadOnlySpan<char> guid, List<string> locations)
	{
		foreach (ReadOnlySpan<char> loc in CollectionsMarshal.AsSpan(locations))
		{
			if (loc.Contains(guid, StringComparison.OrdinalIgnoreCase))
			{
				int index = loc.LastIndexOf(':') + 1;
				return index > 0 && index < loc.Length
					? new(loc.Slice(index).Trim())
					: string.Empty;
			}
		}

		return string.Empty;
	}
}
