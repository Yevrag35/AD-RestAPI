using AD.Api.Ldap.Path;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;

namespace AD.Api.Ldap
{
    public static class EntryFactory
    {
        public static DirectoryEntry GetDirectoryEntry(IPathBuilder builder)
        {
            return new DirectoryEntry(builder.ToPath());
        }
        public static DirectoryEntry GetDirectoryEntry(IPathBuilder builder, NetworkCredential credential)
        {
            return new DirectoryEntry(builder.ToPath(), credential.UserName, credential.Password);
        }
        public static DirectoryEntry GetDirectoryEntry(IPathBuilder builder, NetworkCredential credential, AuthenticationTypes authenticationTypes)
        {
            return new DirectoryEntry(builder.ToPath(), credential.UserName, credential.Password, authenticationTypes);
        }
    }
}
