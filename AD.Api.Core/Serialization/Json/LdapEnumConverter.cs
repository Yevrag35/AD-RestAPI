using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization.Json
{
    public interface ILdapEnumConverterOptions
    {
        ILdapEnumConverterOptions DisallowIntegerValues();
        ILdapEnumConverterOptions DisallowIntegerValues(bool toggle);
        ILdapEnumConverterOptions Exclude<TEnum>() where TEnum : unmanaged, Enum;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        ILdapEnumConverterOptions Exclude(Type type);
        ILdapEnumConverterOptions SetNamingPolicy(JsonNamingPolicy? namingPolicy);
    }

    public sealed class LdapEnumConverter : JsonConverterFactory
    {
        private readonly FrozenSet<Type> _avoidNamingPolicy;
        private readonly JsonStringEnumConverter _defaultPolicyConverter;
        private readonly JsonStringEnumConverter _namingPolicyConverter;

        private LdapEnumConverter(LdapEnumConverterOptions options)
        {
            _avoidNamingPolicy = options.Exclusions.Count > 0 ? options.Exclusions.ToFrozenSet() : FrozenSet<Type>.Empty;
            _namingPolicyConverter = new(options.NamingPolicy, options.AllowIntegerValues);
            _defaultPolicyConverter = new(namingPolicy: null, options.AllowIntegerValues);
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return _avoidNamingPolicy.Contains(typeToConvert)
                ? _defaultPolicyConverter.CreateConverter(typeToConvert, options)
                : _namingPolicyConverter.CreateConverter(typeToConvert, options);
        }

        public static LdapEnumConverter Create(Action<ILdapEnumConverterOptions> configureOptions)
        {
            LdapEnumConverterOptions options = new();
            configureOptions(options);

            return new(options);
        }

        private sealed class LdapEnumConverterOptions : ILdapEnumConverterOptions
        {
            internal bool AllowIntegerValues { get; private set; }
            internal HashSet<Type> Exclusions { get; }
            internal JsonNamingPolicy? NamingPolicy { get; private set; }

            internal LdapEnumConverterOptions()
            {
                this.AllowIntegerValues = true;
                this.Exclusions = [];
            }

            public ILdapEnumConverterOptions DisallowIntegerValues()
            {
                return this.DisallowIntegerValues(toggle: true);
            }
            public ILdapEnumConverterOptions DisallowIntegerValues(bool toggle)
            {
                this.AllowIntegerValues = !toggle;
                return this;
            }
            public ILdapEnumConverterOptions Exclude<TEnum>() where TEnum : unmanaged, Enum
            {
                _ = this.Exclusions.Add(typeof(TEnum));
                return this;
            }
            public ILdapEnumConverterOptions Exclude(Type type)
            {
                ArgumentNullException.ThrowIfNull(type);
                if (!type.IsEnum)
                {
                   throw new ArgumentException("Type must be an enum.", nameof(type));
                }

                _ = this.Exclusions.Add(type);
                return this;
            }
            public ILdapEnumConverterOptions SetNamingPolicy(JsonNamingPolicy? namingPolicy)
            {
                this.NamingPolicy = namingPolicy;
                return this;
            }
        }
    }
}

