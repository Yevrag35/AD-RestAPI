using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Nor : FilterContainer
    {
        public sealed override FilterType Type => FilterType.Nor;

        public Nor()
            : base(2)
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
