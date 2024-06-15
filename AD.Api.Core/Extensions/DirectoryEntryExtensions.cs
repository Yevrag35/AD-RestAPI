using AD.Api.Collections;
using AD.Api.Core.Ldap;
using System.DirectoryServices;
using System.Runtime.Versioning;

namespace AD.Api.Core.Extensions
{
    [SupportedOSPlatform("WINDOWS")]
    public static class DirectoryEntryExtensions
    {
        public static string GetCommonName(this DirectoryEntry entry)
        {
            return entry.Properties.TryGetValue(AttributeConstants.COMMON_NAME, out string? value)
                ? value : string.Empty;
        }
        public static string GetName(this DirectoryEntry entry)
        {
            return entry.Properties.TryGetValue(AttributeConstants.NAME, out string? value)
                ? value : string.Empty;
        }
        public static string GetDistinguishedName(this DirectoryEntry entry)
        {
            return entry.Properties.TryGetValue(AttributeConstants.DISTINGUISHED_NAME, out string? value) 
                ? value : string.Empty;
        }
        public static string[] GetObjectClass(this DirectoryEntry entry)
        {
            return entry.Properties.TryGetValues(AttributeConstants.OBJECT_CLASS, out string[] values) ? values : [];
        }
        public static string GetParentDistinguishedName(this DirectoryEntry entry)
        {
            return GetDistinguishedName(entry.Parent);
        }

        public static bool IsOfClass(this DirectoryEntry entry, string className)
        {
            if (entry.SchemaClassName.Equals(className, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (string cls in entry.Properties[AttributeConstants.OBJECT_CLASS])
            {
                if (className.Equals(cls, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

