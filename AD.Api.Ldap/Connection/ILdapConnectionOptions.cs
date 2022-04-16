using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;

namespace AD.Api.Ldap.Connection
{
    public interface ILdapConnectionOptions
    {
        AuthenticationTypes? AuthenticationTypes { get; }
        string? DistinguishedName { get; }
        bool DontDisposeToken { get; }
        bool IsForest { get; }
        string? Host { get; }
        Protocol Protocol { get; }
        SafeAccessTokenHandle? Token { get; }
        bool UseSchemaCache { get; }
        bool UseSSL { get; }

        NetworkCredential? GetCredential();
    }
}
