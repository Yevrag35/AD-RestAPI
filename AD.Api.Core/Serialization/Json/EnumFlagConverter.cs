using AD.Api.Components;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Serialization.Json
{
    public sealed class EnumFlagConverter : LdapEnumConverter
    {
        internal EnumFlagConverter(LdapEnumConverterOptions options)
            : base(options)
        {
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = base.CreateConverter(typeToConvert, options)!;

            if (!typeToConvert.IsDefined(typeof(FlagsAttribute))
                ||
                !typeof(int).Equals(typeToConvert.GetEnumUnderlyingType()))
            {
                return converter;
            }

            return Create(typeToConvert, converter);
        }

        private static readonly MethodInfo _method = typeof(EnumFlagConverter)
            .GetMethod(nameof(CreateGeneric), BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(nameof(EnumFlagConverter), nameof(CreateGeneric));
        private static JsonConverter Create(Type flagEnumType, JsonConverter converter)
        {
            MethodInfo genMeth = _method.MakeGenericMethod(flagEnumType);
            return (JsonConverter)genMeth.Invoke(null, [converter])!;
        }
        private static JsonConverter<T> CreateGeneric<T>(JsonConverter converter) where T : unmanaged, Enum
        {
            return new FlagEnumArrayConverter<T>(converter);
        }

        private sealed class FlagEnumArrayConverter<T> : JsonConverter<T> where T : unmanaged, Enum
        {
            private readonly JsonConverter<T> _converter;

            internal FlagEnumArrayConverter(JsonConverter enumConverter)
            {
                _converter = (JsonConverter<T>)enumConverter;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    return _converter.Read(ref reader, typeToConvert, options);
                }

                T value = default;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return value;
                    }

                    AppendEnum(ref value, ref reader);
                }

                return value;
            }

            private static void AppendEnum(ref T value, ref readonly Utf8JsonReader reader)
            {
                int count = Encoding.UTF8.GetCharCount(reader.ValueSpan);
                Span<char> buffer = stackalloc char[count];
                int written = Encoding.UTF8.GetChars(reader.ValueSpan, buffer);

                if (Enum.TryParse(buffer.Slice(0, written), ignoreCase: true, out T result))
                {
                    ref int flag = ref Unsafe.As<T, int>(ref result);
                    ref int current = ref Unsafe.As<T, int>(ref value);
                    current |= flag;
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                FlagEnumerator<T> enumerator = new(value);
                while (enumerator.MoveNext())
                {
                    _converter.Write(writer, enumerator.Current, options);
                }

                writer.WriteEndArray();
            }
        }
    }
}

