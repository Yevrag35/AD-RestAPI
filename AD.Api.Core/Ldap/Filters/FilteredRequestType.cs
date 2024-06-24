using AD.Api.Attributes;

namespace AD.Api.Core.Ldap.Filters
{
    [Flags]
    public enum FilteredRequestType
    {
        Any = 0,
        [BackendValue("(objectClass=user)(objectCategory=person)")]
        User = 0x1,
        [BackendValue("(objectClass=computer)(objectCategory=computer)")]
        Computer = 0x2,
        [BackendValue("(objectClass=group)(objectCategory=group)")]
        Group = 0x4,
        [BackendValue("(objectClass=contact)(objectCategory=contact)")]
        Contact = 0x8,

        [BackendValue("(objectClass=container)(objectCategory=container)")]
        Container = 0x10,               // 16
        [BackendValue("(objectClass=organizationalUnit)(objectCategory=organizationalUnit)")]
        OrganizationalUnit = 0x20,      // 32
        [BackendValue("(objectClass=msDS-ManagedServiceAccount)(objectCategory=msDS-ManagedServiceAccount)")]
        ManagedServiceAccount = 0x40,   // 64
    }
}

