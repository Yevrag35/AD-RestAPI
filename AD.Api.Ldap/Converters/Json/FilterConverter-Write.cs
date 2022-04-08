using AD.Api.Ldap.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Converters.Json
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
