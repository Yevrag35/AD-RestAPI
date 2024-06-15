using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Settings;
using AD.Api.Strings.Extensions;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    [SupportedOSPlatform("WINDOWS")]
    public static class SchemaLoader
    {
        public static SchemaClassPropertyDictionary LoadSchema(ConnectionContext context, ReadOnlySpan<string> classNames, in char separator)
        {
            using SchemaDictionaryBuilder builder = new(context);
            foreach (string className in classNames)
            {
                builder.ReadFrom(className);
            }

            return builder.Build();
        }
    }
}

