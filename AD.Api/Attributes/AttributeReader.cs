using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Types;
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
        public static Action<PropertyCollection, (string, object)> GetAction(PropertyInfo propertyInfo)
        {
            if (TryGetAttribute(propertyInfo, out PropertyValueAttribute attribute))
            {
                return attribute.Action;
            }
            else
            {
                throw new ArgumentException(string.Format("{0} is not decorated with a PropertyValueAttribute", propertyInfo.Name));
            }
        }
        public static Action<PropertyCollection, (string, object)> GetAction<T, TProp>(Expression<Func<T, TProp>> propertyExpression)
            where T : IEntry
        {
            if (!TryAsMemberExpression(propertyExpression, out MemberExpression memEx))
                throw new InvalidOperationException("Does not resolve to a proper member");

            else if (TryGetAttribute(memEx.Member, out PropertyValueAttribute attribute))
            {
                return attribute.Action;
            }
            else
            {
                throw new ArgumentException(string.Format("{0} is not decorated with a {1}", memEx.Member.Name,
                    typeof(T).Name));
            }
        }
        public static string GetLdapAttribute(MemberInfo memberInfo)
        {
            if (TryGetAttribute(memberInfo, out LdapAttribute attribute))
            {
                return attribute.Name;
            }
            else
            {
                return null;
            }
        }
        public static string GetLdapAttribute<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            if (TryAsMemberExpression(expression, out MemberExpression memEx) &&
                TryGetAttribute(memEx.Member, out LdapAttribute attribute))
            {
                return attribute.Name;
            }
            else
            {
                throw new ArgumentException(string.Format("{0} is not decorated with a {1}", memEx?.Member.Name,
                    typeof(T).Name));
            }
        }

        private static bool TryGetAttribute<T>(MemberInfo memberInfo, out T propValueAtt)
            where T : Attribute
        {
            propValueAtt = memberInfo.GetCustomAttributes<T>().FirstOrDefault();
            return null != propValueAtt;
        }
        private static bool TryAsMemberExpression<T, TMember>(Expression<Func<T, TMember>> expression, out MemberExpression member)
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

            return null != member;
        }
    }
}
