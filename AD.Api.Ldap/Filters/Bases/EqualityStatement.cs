using AD.Api.Ldap.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public abstract record EqualityStatement : FilterStatementBase
    {
        protected const char STAR = (char)42;

        public abstract string Property { get; }

        //[DisallowNull]
        //protected abstract IConvertible EqualsValue { get; }

        protected abstract string? GetValue();

        protected abstract EqualityStatement ToAny();

        public override StringBuilder WriteTo(StringBuilder builder)
        {
            string? strValue = this.GetValue();

            Func<StringBuilder, StringBuilder> writeValue = string.IsNullOrWhiteSpace(strValue)
                ? sb =>
                {
                    var not = new Not
                    {
                        this.ToAny()
                    };
                    return not.WriteTo(sb);
                }
                : sb =>
                {
                    return sb
                    .Append((char)40) //  (
                    .Append(this.Property)
                    .Append((char)61) // =
                    .Append(strValue)
                    .Append((char)41);
                };

            return writeValue(builder);
        }

        protected static bool TryGetLdapName(MemberExpression expression, [NotNullWhen(true)] out string? ldapPropertyName)
        {
            ldapPropertyName = null;

            if (expression.Member.CustomAttributes.Any(att => att.AttributeType.IsAssignableTo(typeof(LdapPropertyAttribute))
                && att.ConstructorArguments.Any(ca => ca.Value is not null)))
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
