using AD.Api.Ldap.Attributes;
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
    [Obsolete("These extensions are obsolete and will be removed soon.")]
    public static class DirectoryEntryExtensions
    {
        [Obsolete("This extension is obsolete and will be removed soon.")]
        [return: NotNullIfNotNull("directoryEntry")]
        public static T? AsModel<T>(this DirectoryEntry? directoryEntry, ILdapEnumDictionary enumDictionary) where T : new()
        {
            if (directoryEntry is null)
            {
                return default;
            }

            return Mapper.MapFromDirectoryEntry<T>(new T(), directoryEntry, enumDictionary);
        }

        [Obsolete("This extension is obsolete and will be removed soon.")]
        public static LdapUser AsLdapUser(this DirectoryEntry directoryEntry, ILdapEnumDictionary enumDictionary)
        {
            LdapUser user = AsModel<LdapUser>(directoryEntry, enumDictionary);
            user.Path = PathValue.FromDirectoryEntry(directoryEntry);

            return user;
        }
    }
}
