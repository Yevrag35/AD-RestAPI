using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Not : FilterStatementBase
    {
        private readonly EqualityStatement _equalStatement;

        internal EqualityStatement EqualStatement => _equalStatement;
        public sealed override FilterType Type => FilterType.Not;

        public string Property => _equalStatement.Property;

        public string? Value => _equalStatement.GetValue();

        public Not(string property, IConvertible? value)
        {
            _equalStatement = new Equal(property, value);
        }
        internal Not(EqualityStatement equalityStatement)
        {
            _equalStatement = equalityStatement;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(Not), false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();

            _equalStatement.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(!");

            return _equalStatement.WriteTo(builder).Append((char)41);
        }
    }
}
