using AD.Api.Attributes;

namespace AD.Api.Core.Ldap;

[StaticConstantClass]
public static class LdapConstants
{
	public const string LDAP = "LDAP";
	public const string ROOT_DSE = "RootDSE";

	// Object Classes
	public const string OBJ_COMPUTER = "computer";
	public const string OBJ_CONTACT = "contact";
	public const string OBJ_CONTAINER = "container";
	public const string OBJ_GROUP = "group";
	public const string OBJ_MS_DS_MANAGED_SERVICE_ACCOUNT = "msDS-ManagedServiceAccount";
	public const string OBJ_ORGANIZATIONAL_UNIT = "organizationalUnit";
	public const string OBJ_USER = "user";

	// Scope Modifiers
	public const string BITWISE_AND = ":1.2.840.113556.1.4.803:";
	public const string BITWISE_OR = ":1.2.840.113556.1.4.804:";
	public const string RECURSIVE = ":1.2.840.113556.1.4.1941:";
}