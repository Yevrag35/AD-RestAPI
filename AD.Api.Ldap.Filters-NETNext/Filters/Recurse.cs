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
    /// <summary>
    /// A filter that represents an LDAP_MATCHING_RULE_IN_CHAIN extensible match against
    /// a given distinguishedName.
    /// </summary>
#if OLDCODE
    public sealed class Recurse : EqualityStatement
#else
    public sealed record Recurse : EqualityStatement
#endif
    {
        private const string TRANSITIVE_EVAL = "{0}:1.2.840.113556.1.4.1941:";

        public sealed override int Length => base.Length + this.Value.Length;
        public sealed override string Property { get; }
        public sealed override Type? PropertyType => typeof(string);
        public sealed override string RawProperty { get; }
        public sealed override FilterType Type => FilterType.Recurse | FilterType.Equal;

        /// <summary>
        /// The distinguishedName (DN) value to recursively search.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Recurse"/> with the specified property name and distinguishedName.
        /// </summary>
        /// <param name="propertyName">The LDAP property name for the filter.</param>
        /// <param name="distinguishedName">The distinguishedName (DN) who is recursively searched.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="distinguishedName"/> is <see langword="null"/>.
        /// </exception>
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
