using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Connection
{
    public interface ILdapConnectionOptions
    {
        AuthenticationTypes? AuthenticationTypes { get; }
        string? DistinguishedName { get; }
        string? Host { get; }
        Protocol Protocol { get; }
        bool UseSSL { get; }

        NetworkCredential? GetCredential();
    }
}
