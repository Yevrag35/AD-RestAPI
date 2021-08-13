using System;
using AD.Api.Extensions;

namespace AD.Api.Components
{
    public struct LdapPath
    {
        private string _value;

        public string Value => _value;

        public LdapPath(string distinguishedName, string domainController)
        {
            _value = 
        }

        public static implicit operator LdapPath(string distinguishedName)
        {
            return new LdapPath
            {
                _value = distinguishedName.ToLDAPPath()
            };
        }
    }
}
