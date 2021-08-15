using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Types;
using MG.Attributes;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Models.Entries;
using AD.Api.Components;

namespace AD.Api.Attributes
{
    [SupportedOSPlatform("windows")]
    public static class AttributeReader
    {
        private static readonly AttributeValuator<PropertyValueAttribute, Action<PropertyCollection, (string, object)>>
            Evaluator = new AttributeValuator<PropertyValueAttribute, Action<PropertyCollection, (string, object)>>(
                x => x.Action);

        private static readonly AttributeValuator<LdapAttribute, string> LdapEvaluator =
            new AttributeValuator<LdapAttribute, string>(x => x.Name);

        public static Action<PropertyCollection, (string, object)> GetAction(PropertyInfo propertyInfo)
        {
            return Evaluator.GetValue(propertyInfo);
        }
        public static Action<PropertyCollection, (string, object)> GetAction<T, TProp>(Expression<Func<T, TProp>> propertyExpression)
            where T : IEntry
        {
            return Evaluator.GetValue(propertyExpression);
        }
        public static string GetLdapAttribute(MemberInfo memberInfo)
        {
            return LdapEvaluator.GetValue(memberInfo);
        }
        public static string GetLdapAttribute<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            return LdapEvaluator.GetValue(expression);
        }
    }
}
