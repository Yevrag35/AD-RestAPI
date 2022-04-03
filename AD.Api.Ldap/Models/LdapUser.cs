using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Path;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;

namespace AD.Api.Ldap.Models
{
    public class LdapUser : IPathed
    {
        [LdapExtensionData]
        public IDictionary<string, object[]?>? ExtData;

        [LdapProperty("mail")]
        public string? EmailAddress { get; set; }

        [LdapProperty]
        public string? Name { get; set; }

        [LdapIgnore]
        public PathValue? Path { get; set; }

        [LdapProperty]
        public string[]? ProxyAddresses { get; set; }

        [LdapProperty]
        public string? UserPrincipalName { get; set; }

        public LdapUser()
        {
        }

#if DEBUG
        [Obsolete("Only for testing")]
        public Equal GetFilter<T>(string propertyName) where T : IConvertible
        {
            var exp = Expression.Parameter(typeof(LdapUser));
            var param = Expression.Property(exp, propertyName);

            Expression<Func<LdapUser, T?>> e = (Expression<Func<LdapUser, T?>>)Expression.Lambda(param, exp);

            return Equal.Create(this, e);
        }
#endif

        public Equal GetFilter<TMember>(Expression<Func<LdapUser, TMember?>> expression) where TMember : IConvertible
        {
            return Equal.Create(this, expression);
        }
        public static Equal GetFilter<TMember>(IConvertible? value, Expression<Func<LdapUser, TMember?>> expression)
        {
            return Equal.CreateWithValue(value, expression);
        }
    }
}
