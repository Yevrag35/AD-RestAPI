using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    public sealed record BitwiseAnd : EqualityStatement
    {
        private const string BITWISE_AND = "{0}:1.2.840.113556.1.4.803:";

        public sealed override string Property { get; }
        public long Value { get; }

        public BitwiseAnd(string propertyName, long flagValues)
        {
            this.Property = string.Format(BITWISE_AND, propertyName);
            this.Value = flagValues;
        }

        public BitwiseAnd(string propertyName, Enum flagValues)
            : this(propertyName, Convert.ToInt64(flagValues))
        {
        }

        protected sealed override string? GetValue() => Convert.ToString(this.Value);

        protected override EqualityStatement ToAny()
        {
            return this;
        }

        public static BitwiseAnd Create<T, TMember>(T obj, Expression<Func<T, TMember>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember> function = expression.Compile();

            return new BitwiseAnd(propertyName, Convert.ToInt64(function(obj)));
        }
        public static BitwiseAnd Create<T, TMember>(long flagValues, Expression<Func<T, TMember>> expression)
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            return new BitwiseAnd(propertyName, flagValues);
        }
        public static BitwiseAnd Create<T, TMember>(Enum flagValues, Expression<Func<T, TMember>> expression)
        {
            return Create(Convert.ToInt64(flagValues), expression);
        }
    }

    public sealed record BitwiseOr : EqualityStatement
    {
        private const string BITWISE_OR = "{0}:1.2.840.113556.1.4.804:";

        public sealed override string Property { get; }
        public long Value { get; }

        public BitwiseOr(string propertyName, long flagValues)
        {
            this.Property = string.Format(BITWISE_OR, propertyName);
            this.Value = flagValues;
        }
        public BitwiseOr(string propertyName, Enum flagValues)
            : this(propertyName, Convert.ToInt64(flagValues))
        {
        }

        protected sealed override string? GetValue() => Convert.ToString(this.Value);

        protected sealed override EqualityStatement ToAny()
        {
            return this;
        }

        public static BitwiseOr Create<T, TMember>(T obj, Expression<Func<T, TMember>> expression) where TMember : IConvertible
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
                ? ldapName
                : memberExpression.Member.Name;

            Func<T, TMember> function = expression.Compile();

            return new BitwiseOr(propertyName, Convert.ToInt64(function(obj)));
        }
        public static BitwiseOr Create<T, TMember>(long flagValues, Expression<Func<T, TMember>> expression)
        {
            if (!TryAsMemberExpression(expression, out MemberExpression? memberExpression))
                throw new ArgumentException($"{nameof(expression)} is not a valid {nameof(MemberExpression)}.");

            string propertyName = TryGetLdapName(memberExpression, out string? ldapName)
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
