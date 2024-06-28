using AD.Api.Attributes;

namespace AD.Api.Core.Ldap
{
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
    }
}
