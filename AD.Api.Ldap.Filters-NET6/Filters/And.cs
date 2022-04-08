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
    /// A filter record which indicates all statements contained must be <see langword="true"/>.
    /// </summary>
    public sealed record And : FilterContainer
    {
        public override FilterType Type => FilterType.And;

        /// <summary>
        /// Initializes a new instance of <see cref="And"/> with the default capacity of 2.
        /// </summary>
        public And()
            : base(2)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="And"/> with the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of statements the instance of <see cref="And"/> can initially hold.</param>
        public And(int capacity)
            : base(capacity)
        {
        }

        public sealed override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(And).ToLower());
            writer.WriteStartObject();

            base.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(&");

            return base
                .WriteTo(builder)
                .Append((char)41);
        }
    }
}
