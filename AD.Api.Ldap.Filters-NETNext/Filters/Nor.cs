using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// A filter class which indicates all statements contained must NOT be <see langword="true"/>.
    /// </summary>
#if OLDCODE
    public sealed class Nor : FilterContainer
#else
    public sealed record Nor : FilterContainer
#endif
    {
        public sealed override int Length => base.Length + 3;

        public sealed override FilterType Type => FilterType.Nor;

        /// <summary>
        /// Initializes a new instance of <see cref="Nor"/> with the default capacity of 2.
        /// </summary>
        public Nor()
            : base(2)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Nor"/> with the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of statements the instance of <see cref="Nor"/> can initially hold.</param>
        public Nor(int capacity)
            : base(capacity)
        {
        }

        public sealed override void Add(IFilterStatement? statement)
        {
            switch (statement?.Type)
            {
                case FilterType.Equal:
                    Equal equals = (Equal)statement;
                    base.Add(new Not(equals.Property, equals.Value));
                    break;

                case FilterType.Not:
                    base.Add(statement);
                    break;

                default:
                    break;
            }
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(Nor), false);
            writer.WritePropertyName(name);

            writer.WriteStartArray();

            this.Clauses.ForEach(clause =>
            {
                writer.WriteStartObject();
                if (clause is Not not)
                {
                    not.EqualStatement.WriteTo(writer, strategy, serializer);
                }
                else
                {
                    clause.WriteTo(writer, strategy, serializer);
                }

                writer.WriteEndObject();
            });

            writer.WriteEndArray();
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {   
            builder.Append("(&");
            return base.WriteTo(builder).Append((char)41);
        }
    }
}
