using AD.Api.Reflection;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    public readonly record struct SchemaProperty : ISchemaProperty
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _isNotEmpty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _name;
        private readonly Type _type;

        public readonly bool IsEmpty => !_isNotEmpty;
        public string Name => _name ?? string.Empty;
        public Type RuntimeType => _type ?? typeof(object);

        [DebuggerStepThrough]
        public SchemaProperty(string name, Type type)
        {
            _name = name;
            _type = type;
            _isNotEmpty = true;
        }

        [SupportedOSPlatform("WINDOWS")]
        public static SchemaProperty Create(ActiveDirectorySchemaProperty property)
        {
            return new(property.Name, GetConversionType(property.Syntax, property.IsSingleValued));
        }
        [SupportedOSPlatform("WINDOWS")]
        public static SchemaProperty Create(string name, ActiveDirectorySyntax syntax, bool isSingleValued)
        {
            return new(name, GetConversionType(syntax, isSingleValued));
        }

        [SupportedOSPlatform("WINDOWS")]
        private static Type GetConversionType(ActiveDirectorySyntax syntax, bool isSingleValued)
        {
            Type type = syntax switch
            {
                ActiveDirectorySyntax.AccessPointDN => typeof(string),
                ActiveDirectorySyntax.Bool => typeof(bool?),
                ActiveDirectorySyntax.CaseExactString => typeof(string),
                ActiveDirectorySyntax.CaseIgnoreString => typeof(string),
                ActiveDirectorySyntax.DirectoryString => typeof(string),
                ActiveDirectorySyntax.DN => typeof(string),
                ActiveDirectorySyntax.DNWithBinary => typeof(Guid),
                ActiveDirectorySyntax.DNWithString => typeof(string),
                ActiveDirectorySyntax.Enumeration => typeof(int),
                ActiveDirectorySyntax.GeneralizedTime => typeof(DateTimeOffset),
                ActiveDirectorySyntax.IA5String => typeof(string),
                ActiveDirectorySyntax.Int => typeof(int),
                ActiveDirectorySyntax.Int64 => typeof(long),
                ActiveDirectorySyntax.NumericString => typeof(string),
                ActiveDirectorySyntax.OctetString => typeof(byte[]),
                ActiveDirectorySyntax.Oid => typeof(string),
                ActiveDirectorySyntax.PresentationAddress => typeof(byte[]),
                ActiveDirectorySyntax.PrintableString => typeof(string),
                ActiveDirectorySyntax.ReplicaLink => typeof(byte[]),
                ActiveDirectorySyntax.SecurityDescriptor => typeof(byte[]),
                ActiveDirectorySyntax.Sid => typeof(byte[]),
                ActiveDirectorySyntax.UtcTime => typeof(DateTimeOffset),
                
                _ => typeof(object),
            };

            if (!isSingleValued)
            {
                
                type = type.MakeArrayType();
            }

            return type;
        }
    }
}

