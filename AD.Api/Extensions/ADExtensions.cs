using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Extensions
{
    public static class DirectorySearcherExtensions
    {
        private static readonly StringBuilder _builder = new StringBuilder(18);

        public static string ToLdapPath(this string str)
        {
            return ToLdapPath(str, null);
        }
        public static string ToLdapPath(this string dn, string domainController)
        {
            if (string.IsNullOrEmpty(dn))
                return string.Empty;

            int dcLength = 0;
            if (!string.IsNullOrEmpty(domainController))
                dcLength = domainController.Length;

            int total = dn.Length + dcLength + Strings.LDAP_Prefix.Length;

            _builder.Clear().EnsureCapacity(total);
            return _builder
                .Append(Strings.LDAP_Prefix)
                .Append(domainController)
                .Append(dn)
                .ToString();
        }
    }
}
