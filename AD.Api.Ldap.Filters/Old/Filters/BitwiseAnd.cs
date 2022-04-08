using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// A filter class that represents an LDAP bitwise AND operation.
    /// </summary>
    public sealed class BitwiseAnd : EqualityStatement
    {
        private const string BITWISE_AND = "{0}:1.2.840.113556.1.4.803:";

        public sealed override string Property { get; }
        public sealed override Type PropertyType => typeof(long);
        public sealed override string RawProperty { get; }
        public sealed override FilterType Type => FilterType.Band;
        /// <summary>
        /// The enumeration <see cref="long"/> value.
        /// </summary>
        public long Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="BitwiseAnd"/> with the specified property and <see cref="long"/> value.
        /// </summary>
        /// <param name="propertyName">The LDAP property name.</param>
        /// <param name="flagValues">The enumeration value to equal (as <see cref="long"/>).</param>
        public BitwiseAnd(string propertyName, long flagValues)
        {
            this.RawProperty = propertyName;
            this.Property = string.Format(BITWISE_AND, propertyName);
            this.Value = flagValues;
        }
        /// <summary>
        /// /// Initializes a new instance of <see cref="BitwiseAnd"/> with the specified property and <see cref="Enum"/> value.
        /// </summary>
        /// <param name="propertyName">The LDAP property name.</param>
        /// <param name="flagValues">The enumeration flag values to equal.</param>
        public BitwiseAnd(string propertyName, Enum flagValues)
            : this(propertyName, Convert.ToInt64(flagValues))
        {
        }

        protected sealed override object GetRawValue() => this.Value;
        protected internal sealed override string GetValue() => Convert.ToString(this.Value);

        protected override EqualityStatement ToAny()
        {
            return this;
        }

        public sealed override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(FilterType.Band), false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();

            base.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }

        public static BitwiseAnd Create<T, TMember>(T obj, Expression<Func<T, TMember>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember> function = expression.Compile();

            return new BitwiseAnd(propertyName, Convert.ToInt64(function(obj)));
        }
        public static BitwiseAnd Create<T, TMember>(long flagValues, Expression<Func<T, TMember>> expression)
        {
            if (!TryAsMemberExpression(expression, out MemberExpression memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            return new BitwiseAnd(propertyName, flagValues);
        }
        public static BitwiseAnd Create<T, TMember>(Enum flagValues, Expression<Func<T, TMember>> expression)
        {
            return Create(Convert.ToInt64(flagValues), expression);
        }
    }
}
