using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// A filter record which indicates any statements contained must be <see langword="true"/>.
    /// </summary>
    public sealed record Or : FilterContainer
    {
        public sealed override FilterType Type => FilterType.Or;

        /// <summary>
        /// Initializes a new instance of <see cref="Or"/> with the default capacity of 2.
        /// </summary>
        public Or()
            : base(2)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Or"/> with the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of statements the instance of <see cref="Or"/> can initially hold.</param>
        public Or(int capacity)
            : base(capacity)
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
