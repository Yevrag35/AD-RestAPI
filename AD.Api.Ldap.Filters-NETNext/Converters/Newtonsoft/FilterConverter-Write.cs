using AD.Api.Ldap.Filters;
using Newtonsoft.Json;
using System;

namespace AD.Api.Ldap.Converters
{
    public partial class FilterConverter
    {
        public override void WriteJson(JsonWriter writer, IFilterStatement? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            value?.WriteTo(writer, this.NamingStrategy, serializer);

            writer.WriteEndObject();
        }
    }
}
