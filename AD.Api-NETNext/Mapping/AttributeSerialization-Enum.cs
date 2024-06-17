using AD.Api.Core.Ldap;
using AD.Api.Core.Serialization;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        private const string FLAGS = "Flags";

        public static void WriteEnumValue<TEnum>(Utf8JsonWriter writer, ref readonly SerializationContext context) where TEnum : unmanaged, Enum
        {
            bool isFlag = typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false);
            if (context.Value is not int intVal)
            {
                writer.WriteNullValue();
                return;
            }

            if (isFlag)
            {
                writer.WriteNumberValue(intVal);
                Span<char> chars = stackalloc char[context.AttributeName.Length + 5];
                context.AttributeName.CopyTo(chars);
                FLAGS.CopyTo(chars.Slice(context.AttributeName.Length));

                writer.WritePropertyName(chars);
            }

            Type enumType = typeof(TEnum);
            object enumVal = Enum.ToObject(enumType, intVal);
            JsonSerializer.Serialize(writer, enumVal, enumType, context.Options);
        }
    }
}
