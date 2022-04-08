using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq.Expressions;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// A filter class that represents an LDAP bitwise OR operation.
    /// </summary>
    public sealed class BitwiseOr : EqualityStatement
    {
        private const string BITWISE_OR = "{0}:1.2.840.113556.1.4.804:";

        public sealed override string Property { get; }
        public sealed override Type PropertyType => typeof(long);
        public sealed override string RawProperty { get; }
        public sealed override FilterType Type => FilterType.Bor;
        /// <summary>
        /// The enumeration <see cref="long"/> value.
        /// </summary>
        public long Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="BitwiseOr"/> with the specified property and <see cref="long"/> value.
        /// </summary>
        /// <param name="propertyName">The LDAP property name.</param>
        /// <param name="flagValues">The enumeration value to equal (as <see cref="long"/>).</param>
        public BitwiseOr(string propertyName, long flagValues)
        {
            this.RawProperty = propertyName;
            this.Property = string.Format(BITWISE_OR, propertyName);
            this.Value = flagValues;
        }
        /// <summary>
        /// /// Initializes a new instance of <see cref="BitwiseOr"/> with the specified property and <see cref="Enum"/> value.
        /// </summary>
        /// <param name="propertyName">The LDAP property name.</param>
        /// <param name="flagValues">The enumeration flag values to equal.</param>
        public BitwiseOr(string propertyName, Enum flagValues)
            : this(propertyName, Convert.ToInt64(flagValues))
        {
        }

        protected sealed override object GetRawValue() => this.Value;
        protected internal sealed override string GetValue() => Convert.ToString(this.Value);

        protected sealed override EqualityStatement ToAny()
        {
            return this;
        }

        public sealed override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            string name = strategy.GetPropertyName(nameof(FilterType.Bor), false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();

            base.WriteTo(writer, strategy, serializer);

            writer.WriteEndObject();
        }

        public static BitwiseOr Create<T, TMember>(T obj, Expression<Func<T, TMember>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember> function = expression.Compile();

            return new BitwiseOr(propertyName, Convert.ToInt64(function(obj)));
        }
        public static BitwiseOr Create<T, TMember>(long flagValues, Expression<Func<T, TMember>> expression)
        {
            if (!TryAsMemberExpression(expression, out MemberExpression memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            return new BitwiseOr(propertyName, flagValues);
        }
        public static BitwiseOr Create<T, TMember>(Enum flagValues, Expression<Func<T, TMember>> expression)
        {
            return Create(Convert.ToInt64(flagValues), expression);
        }
    }
}
