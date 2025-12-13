using AD.Api.Enums;

namespace AD.Api.Core.Ldap.Filters;

public static class FilteredRequestTypeExtensions
{
	public static string GetObjectClass(this FilteredRequestType type)
	{
		foreach (FilteredRequestType flag in type.EnumerateFlags())
		{
			return flag switch
			{
				FilteredRequestType.User => LdapConstants.OBJ_USER,
				FilteredRequestType.Computer => LdapConstants.OBJ_COMPUTER,
				FilteredRequestType.Group => LdapConstants.OBJ_GROUP,
				FilteredRequestType.Contact => LdapConstants.OBJ_CONTACT,
				FilteredRequestType.Container => LdapConstants.OBJ_CONTAINER,
				FilteredRequestType.OrganizationalUnit => LdapConstants.OBJ_ORGANIZATIONAL_UNIT,
				FilteredRequestType.ManagedServiceAccount => LdapConstants.OBJ_MS_DS_MANAGED_SERVICE_ACCOUNT,
				_ => string.Empty,
			};
		}

		return string.Empty;
	}
}

