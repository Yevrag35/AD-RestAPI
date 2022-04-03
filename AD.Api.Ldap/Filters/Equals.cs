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
    public sealed record Equal : EqualityStatement
    {
        public sealed override string Property { get; }
        public string? Value { get; }

        public Equal(string propertyName, IConvertible? value)
        {
            this.Property = propertyName;
            this.Value = value is string strValue
                ? strValue
                : Convert.ToString(value);
        }

        protected sealed override string? GetValue() => this.Value;

        public static Equal Create<T, TMember>(T obj, Expression<Func<T, TMember?>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember?> function = expression.Compile();

            return new Equal(propertyName, function(obj));
        }
        public static Equal CreateWithValue<T, TMember>(IConvertible? value, Expression<Func<T, TMember?>> expression)
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
