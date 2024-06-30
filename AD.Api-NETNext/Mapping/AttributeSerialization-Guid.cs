using AD.Api.Core.Serialization;
using AD.Api.Statics;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace AD.Api.Mapping
{
    public static partial class AttributeSerialization
    {
        public static void WriteGuid(Utf8JsonWriter writer, ref readonly SerializationContext context)
        {
            if (context.Value is not byte[] byteArray || byteArray.Length != 16)
            {
                writer.WriteNullValue();
                return;
            }

            Guid guid = new Guid(byteArray.AsSpan());
            Span<char> chars = stackalloc char[LengthConstants.GUID_FORM_D];
            _ = guid.TryFormat(chars, out int written);
            writer.WriteStringValue(chars.Slice(0, written));
        }
    }
}
