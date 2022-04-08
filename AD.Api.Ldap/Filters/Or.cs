using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Or : FilterContainer
    {
        public sealed override FilterType Type => FilterType.Or;

        public Or()
            : base(2)
        {
        }

        public sealed override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(Or), false);
            writer.WritePropertyName(name);
            writer.WriteStartArray();

            if (this.Count > 0)
            {
                this.Clauses.ForEach(clause =>
                {
                    writer.WriteStartObject();

                    clause.WriteTo(writer, strategy, serializer);

                    writer.WriteEndObject();
                });
            }

            writer.WriteEndArray();
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(|");

            return base
                .WriteTo(builder)
                .Append((char)41);
        }
    }
}
