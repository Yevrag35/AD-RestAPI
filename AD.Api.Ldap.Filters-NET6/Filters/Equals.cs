using AD.Api.Ldap.Attributes;
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
    /// A filter record that represents an LDAP equal statement using a given property/value.
    /// </summary>
#if OLDCODE
    public sealed class Equal : EqualityStatement
#else
    public sealed record Equal : EqualityStatement
#endif
    {
        public sealed override string Property { get; }
        public sealed override Type? PropertyType { get; }
        public sealed override string RawProperty => this.Property;
        public sealed override FilterType Type => FilterType.Equal;
        /// <summary>
        /// The converted <see cref="string"/> value the property must equal.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="Equal"/> with the specified property name and value.
        /// </summary>
        /// <param name="propertyName">The LDAP property name.</param>
        /// <param name="value">The value to equal when converted to a <see cref="string"/>.</param>
        public Equal(string propertyName, IConvertible? value)
        {
            this.Property = propertyName;
            this.PropertyType = value?.GetType();
            string? v = value is string strValue
                ? strValue
                : Convert.ToString(value);

            this.Value = !string.IsNullOrWhiteSpace(v)
                ? v : null;
        }

        protected sealed override object? GetRawValue() => this.Value;
        protected internal sealed override string? GetValue() => this.Value;

        /// <summary>
        /// Returns a new <see cref="Equal"/> instance with the same property name, but with a value of '*'
        /// to indicate an ALL wildcard.
        /// </summary>
        /// <returns>
        ///     A new instance of <see cref="Equal"/>.
        /// </returns>
        public Equal Any()
        {
            return new Equal(this.RawProperty, STAR);
        }

        protected sealed override EqualityStatement ToAny()
        {
            return this.Any();
        }

        public static Equal Create<T, TMember>(T obj, Expression<Func<T, TMember>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember> function = expression.Compile();

            return new Equal(propertyName, function(obj));
        }
        public static Equal CreateWithValue<T, TMember>(IConvertible? value, Expression<Func<T, TMember>> expression)
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            return new Equal(propertyName, value);
        }
    }
}
