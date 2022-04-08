using AD.Api.Ldap.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// An <see langword="abstract"/> base statement record that specifies equality in an LDAP filter.
    /// </summary>
#if OLDCODE
    public abstract class EqualityStatement : FilterStatementBase
#else
    public abstract record EqualityStatement : FilterStatementBase
#endif
    {
        protected const char STAR = (char)42;

        /// <summary>
        /// The property name without any transformations.
        /// </summary>
        /// <example>
        ///     If Property equals 'memberOf:1.2.840.113556.1.4.1941:',
        ///     then RawProperty will equal 'memberOf'
        /// </example>
        public abstract string RawProperty { get; }

        /// <summary>
        /// The property name used in the LDAP filter.
        /// </summary>
        public abstract string Property { get; }

        /// <summary>
        /// The underlying type of the property value prior to converting to a <see cref="string"/>.
        /// </summary>
        public abstract Type? PropertyType { get; }

        /// <summary>
        /// Determines if the current <see cref="EqualityStatement"/> equals the specified <see cref="IFilterStatement"/> 
        /// implementation.
        /// </summary>
        /// <remarks>
        ///     Checks for equality based on <see cref="FilterStatementBase.Type"/>, <see cref="Property"/>,
        ///     and the result of <see cref="GetValue"/> are all equal.
        /// </remarks>
        /// <param name="other">The implementation to check equality against.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="other"/> is equal to the current instance of <see cref="EqualityStatement"/>;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public sealed override bool Equals(IFilterStatement? other)
        {
            return
                base.Equals(other)
                &&
                other is EqualityStatement eqState
                &&
                StringComparer.CurrentCultureIgnoreCase.Equals(this.Property, eqState.Property)
                &&
                StringComparer.CurrentCultureIgnoreCase.Equals(this.GetValue(), eqState.GetValue());
        }

        protected internal abstract string? GetValue();
        protected abstract object? GetRawValue();

        protected abstract EqualityStatement ToAny();

        /// <summary>
        /// Serializes the <see cref="EqualityStatement"/> into JSON and writes it to the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write the JSON to.</param>
        /// <param name="strategy">The naming strategy for property names.</param>
        /// <param name="serializer">The serializer used when serializing values.</param>
        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(this.RawProperty, false);
            writer.WritePropertyName(name);
            object? rawValue = this.GetRawValue();
            JToken token = !(rawValue is null)
                ? JToken.FromObject(rawValue)
                : JValue.CreateNull();

            if (token.Type == JTokenType.Null || this.PropertyType is null)
            {
                writer.WriteNull();
                return;
            }

            serializer.Serialize(writer, token);
        }

        public override StringBuilder WriteTo(StringBuilder builder)
        {
            string? strValue = this.GetValue();

            Func<StringBuilder, StringBuilder> writeValue;
            if (!string.IsNullOrWhiteSpace(strValue))
            {
                writeValue = stringBuilder =>
                {
                    return stringBuilder
                        .Append((char)40) //  (
                        .Append(this.Property)
                        .Append((char)61) // =
                        .Append(strValue)
                        .Append((char)41); // )
                };
            }
            else
            {
                writeValue = stringBuilder =>
                {
                    var not = new Not(this.ToAny());
                    return not.WriteTo(stringBuilder);
                };
            }

            return writeValue(builder);
        }

        protected static bool TryGetLdapName(MemberExpression expression, [NotNullWhen(true)] out string? ldapPropertyName)
        {
            ldapPropertyName = null;

            if (expression.Member.CustomAttributes.Any(att => typeof(LdapPropertyAttribute).IsAssignableFrom(att.AttributeType)
                && att.ConstructorArguments.Any(ca => !(ca.Value is null))))
            {
                ldapPropertyName = expression.Member.GetCustomAttribute<LdapPropertyAttribute>()?.LdapName;
            }

            return !string.IsNullOrWhiteSpace(ldapPropertyName);
        }

        protected static bool TryAsMemberExpression<T, TMember>(Expression<Func<T, TMember>> expression, 
            [NotNullWhen(true)] out MemberExpression? member)
        {
            member = null;
            if (expression?.Body is MemberExpression memEx)
            {
                member = memEx;
            }
            else if (expression?.Body is UnaryExpression unEx && unEx.Operand is MemberExpression unExMem)
            {
                member = unExMem;
            }

            return member != null;
        }
    }
}