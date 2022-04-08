using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Recurse : EqualityStatement
    {
        private const string TRANSITIVE_EVAL = "{0}:1.2.840.113556.1.4.1941:";

        public sealed override string Property { get; }
        public sealed override Type? PropertyType => typeof(string);
        public sealed override string RawProperty { get; }
        public sealed override FilterType Type => FilterType.Recurse | FilterType.Equal;
        public string Value { get; }

        public Recurse(string propertyName, string distinguishedName)
            : base()
        {
            this.RawProperty = propertyName;
            this.Property = string.Format(TRANSITIVE_EVAL, propertyName);
            this.Value = distinguishedName ?? throw new ArgumentNullException(nameof(distinguishedName));
        }

        protected sealed override object? GetRawValue() => this.Value;

        [return: NotNull]
        protected internal sealed override string? GetValue()
        {
            return this.Value;
        }
        protected override EqualityStatement ToAny()
        {
            return this;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(Recurse), false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();

            base.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }
    }
}
