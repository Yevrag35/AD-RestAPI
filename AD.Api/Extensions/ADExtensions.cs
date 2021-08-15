using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Extensions
{
    public static class DirectorySearcherExtensions
    {
        private static readonly StringBuilder _builder = new StringBuilder(18);

        public static string ToLdapPath(this string str)
        {
            return ToLdapPath(str, null, null);
        }
        public static string ToLdapPath(this string dn, string domainController)
        {
            if (string.IsNullOrWhiteSpace(dn))
            {
                return string.Empty;
            }

            int dcLength = (domainController?.Length).GetValueOrDefault();
            int domainLength = (dn?.Length).GetValueOrDefault();

            int total = Strings.LDAP_Prefix.Length + domainLength + dcLength;

            _builder.Clear().EnsureCapacity(total);
            return _builder
                .Append(Strings.LDAP_Prefix)
                .Append(domainController)
                .Append(dn)
                .ToString();
        }
        public static string ToLdapPath(this string nonLdapPath, string domainDN, string domainController)
        {
            if (string.IsNullOrWhiteSpace(nonLdapPath) && string.IsNullOrWhiteSpace(domainDN))
            {
                return string.Empty;
            }

            int nonLdapLength = (nonLdapPath?.Length).GetValueOrDefault();
            int dcLength =      (domainController?.Length).GetValueOrDefault();
            int domainLength =  (domainDN?.Length).GetValueOrDefault();

            int total = Strings.LDAP_Prefix.Length + nonLdapLength + domainLength + dcLength;

            _builder.Clear().EnsureCapacity(total);
            return _builder
                .Append(Strings.LDAP_Prefix)
                .Append(domainController)
                .Append(nonLdapLength)
                .Append(domainDN)
                .ToString();
        }
    }
}
