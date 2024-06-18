using AD.Api.Core.Serialization;
using AD.Api.Enums;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        private const string FLAGS = "Flags";

        public static void WriteEnumValue<TEnum>(Utf8JsonWriter writer, ref readonly SerializationContext context) where TEnum : unmanaged, Enum
        {
            if (context.Value is not int intVal)
            {
                writer.WriteNullValue();
                return;
            }

            var dict = context.Services.GetRequiredService<IEnumStrings<TEnum>>();

            TEnum enumVal = default;
            if (dict.IsFlagsEnum)
            {
                writer.WriteNumberValue(intVal);
                Span<char> chars = stackalloc char[context.AttributeName.Length + 5];
                context.AttributeName.CopyTo(chars);
                FLAGS.CopyTo(chars.Slice(context.AttributeName.Length));

                writer.WritePropertyName(chars);

                enumVal = ParseFromInt<TEnum>(ref intVal);
            }
            else if (!dict.TryGetEnum(intVal, out enumVal))
            {
                writer.WriteNumberValue(intVal);
                return;
            }

            JsonSerializer.Serialize(writer, enumVal, context.Options);
        }

        [DebuggerStepThrough]
        private static TEnum ParseFromInt<TEnum>(ref int intValue) where TEnum : unmanaged, Enum
        {
            return Unsafe.As<int, TEnum>(ref intValue);
        }
    }
}
