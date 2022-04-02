using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Path
{
    public static class ConnectionPathFactory
    {
        public static IPathBuilder GetGlobalCatalogPath(string domainOrDc)
        {
            return new GlobalCatalogUriBuilder(domainOrDc);
        }
        public static IPathBuilder GetGlobalCatalogPath(string domainOrDc, string distinguishedName)
        {
            return GetGlobalCatalogPath(domainOrDc, distinguishedName, false);
        }
        public static IPathBuilder GetGlobalCatalogPath(string domainOrDc, string distinguishedName, bool useSSL)
        {
            return new GlobalCatalogUriBuilder(domainOrDc)
            {
                DistinguishedName = distinguishedName,
                SSL = useSSL
            };
        }
        public static IPathBuilder GetLdapPath(string domainOrDc)
        {
            return new LdapUriBuilder(domainOrDc);
        }
        public static IPathBuilder GetLdapPath(string domainOrDc, string distinguishedName)
        {
            return GetLdapPath(domainOrDc, distinguishedName, false);
        }
        public static IPathBuilder GetLdapPath(string domainOrDc, string distinguishedName, bool useSSL)
        {
            return new LdapUriBuilder(domainOrDc)
            {
                DistinguishedName = distinguishedName,
                SSL = useSSL
            };
        }
    }
}
