using AD.Api.Core.Ldap;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    public readonly record struct SchemaProperty : ISchemaProperty
    {
        public static readonly Type ByteArrayType = typeof(byte[]);
        public static readonly Type DateTimeType = typeof(DateTimeOffset);
        public static readonly Type GuidType = typeof(Guid);
        public static readonly Type IntType = typeof(int);
        public static readonly Type LongType = typeof(long);
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type StringType = typeof(string);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _isMulti;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _isNotEmpty;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly LdapValueType _ldapType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _name;
        private readonly Type _type;

        public readonly bool IsEmpty => !_isNotEmpty;
        public readonly bool IsMulti => _isMulti;
        public readonly LdapValueType LdapType => _ldapType;
        public string Name => _name ?? string.Empty;
        public Type RuntimeType => _type ?? ObjectType;

        [DebuggerStepThrough]
        public SchemaProperty(string name, Type type, LdapValueType ldapType, bool isSingle)
        {
            _name = name;
            _type = type;
            _isNotEmpty = true;
            _ldapType = ldapType;
            _isMulti = !isSingle;
        }

        [SupportedOSPlatform("WINDOWS")]
        public static SchemaProperty Create(ActiveDirectorySchemaProperty property)
        {
            (Type type, LdapValueType ldapType) = GetConversionType(property.Syntax, property.IsSingleValued);
            return new(property.Name, type, ldapType, property.IsSingleValued);
        }
        [SupportedOSPlatform("WINDOWS")]
        public static SchemaProperty Create(string name, ActiveDirectorySyntax syntax, bool isSingleValued)
        {
            (Type type, LdapValueType ldapType) = GetConversionType(syntax, isSingleValued);
            return new(name, type, ldapType, isSingleValued);
        }

        [SupportedOSPlatform("WINDOWS")]
        private static (Type, LdapValueType) GetConversionType(ActiveDirectorySyntax syntax, bool isSingleValued)
        {
            (Type Type, LdapValueType LdapType) tuple = MatchSyntaxToTypes(syntax);

            if (!isSingleValued)
            {
                tuple.Type = tuple.Type.MakeArrayType();
                tuple.LdapType = tuple.LdapType switch
                {
                    LdapValueType.Boolean => LdapValueType.BooleanArray,
                    LdapValueType.ByteArray => LdapValueType.ByteTwoRankArray,
                    LdapValueType.DateTime => LdapValueType.DateTimeArray,
                    LdapValueType.String => LdapValueType.StringArray,
                    LdapValueType.Integer => LdapValueType.IntegerArray,
                    LdapValueType.Long => LdapValueType.LongArray,
                    LdapValueType.Guid => LdapValueType.GuidArray,
                    _ => LdapValueType.ObjectArray,
                };
            }

            return tuple;
        }

        [SupportedOSPlatform("WINDOWS")]
        private static (Type type, LdapValueType ldapType) MatchSyntaxToTypes(ActiveDirectorySyntax syntax)
        {
            switch (syntax)
            {
                case ActiveDirectorySyntax.AccessPointDN:
                case ActiveDirectorySyntax.CaseExactString:
                case ActiveDirectorySyntax.CaseIgnoreString:
                case ActiveDirectorySyntax.DirectoryString:
                case ActiveDirectorySyntax.DN:
                case ActiveDirectorySyntax.DNWithString:
                case ActiveDirectorySyntax.IA5String:
                case ActiveDirectorySyntax.NumericString:
                case ActiveDirectorySyntax.Oid:
                case ActiveDirectorySyntax.ORName:
                case ActiveDirectorySyntax.PrintableString:
                    return (StringType, LdapValueType.String);

                case ActiveDirectorySyntax.Bool:
                    return (typeof(bool?), LdapValueType.Boolean);

                //case ActiveDirectorySyntax.DNWithBinary:
                //    return (GuidType, LdapValueType.Guid);

                case ActiveDirectorySyntax.Enumeration:
                case ActiveDirectorySyntax.Int:
                    return (IntType, LdapValueType.Integer);

                case ActiveDirectorySyntax.Int64:
                    return (LongType, LdapValueType.Long);

                case ActiveDirectorySyntax.GeneralizedTime:
                case ActiveDirectorySyntax.UtcTime:
                    return (DateTimeType, LdapValueType.DateTime);

                case ActiveDirectorySyntax.DNWithBinary:
                case ActiveDirectorySyntax.OctetString:
                case ActiveDirectorySyntax.PresentationAddress:
                case ActiveDirectorySyntax.ReplicaLink:
                case ActiveDirectorySyntax.SecurityDescriptor:
                case ActiveDirectorySyntax.Sid:
                    return (ByteArrayType, LdapValueType.ByteArray);

                default:
                    return (ObjectType, LdapValueType.Object);
            }
        }
    }
}

