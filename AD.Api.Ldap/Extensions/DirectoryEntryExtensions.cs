using AD.Api.Ldap.Mapping;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Path;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Extensions
{
    public static class DirectoryEntryExtensions
    {
        [return: NotNullIfNotNull("directoryEntry")]
        public static T? AsModel<T>(this DirectoryEntry? directoryEntry) where T : new()
        {
            if (directoryEntry is null)
                return default;

            return Mapper.MapFromDirectoryEntry<T>(new T(), directoryEntry);
        }

        public static LdapUser AsLdapUser(this DirectoryEntry directoryEntry)
        {
            LdapUser user = AsModel<LdapUser>(directoryEntry);
            user.Path = PathValue.FromDirectoryEntry(directoryEntry);

            return user;
        }
    }
}
