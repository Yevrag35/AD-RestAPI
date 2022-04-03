using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Types;
using MG.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using AD.Api.Models.Entries;
using AD.Api.Components;
using AD.Api.Extensions;

using Strings = AD.Api.Properties.Resource;

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

        private static readonly AttributeValuator<JsonPropertyAttribute, string> JsonPropertyEvaluator =
            new AttributeValuator<JsonPropertyAttribute, string>(
                x => x.PropertyName);

        private static readonly AttributeValuator EnumEvaluator = new AttributeValuator();

        public static Action<PropertyCollection, (string, object)> GetAction(PropertyInfo propertyInfo)
        {
            return Evaluator.GetValue(propertyInfo);
        }
        public static Action<PropertyCollection, (string, object)> GetAction<T, TProp>(Expression<Func<T, TProp>> propertyExpression)
            where T : IEntry
        {
            return Evaluator.GetValue(propertyExpression);
        }
        public static T GetAdditionalValue<T>(Enum enumeration)
        {
            return EnumEvaluator.GetAttributeValue<T, AdditionalValueAttribute>(enumeration);
        }
        public static MemberInfo[] GetJsonProperties<T>(bool includeFields = false)
            where T : class
        {
            var memInfos = new List<MemberInfo>(10);
            Type type = typeof(T);

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (null != props)
                memInfos.AddRange(props);

            if (includeFields)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                if (null != fields)
                    memInfos.AddRange(fields);
            }

            return memInfos.ToArray();
        }
        public static string GetJsonValue(MemberInfo memberInfo)
        {
            return JsonPropertyEvaluator.GetValue(memberInfo);
        }
        public static string GetJsonValue<TClass, TProp>(Expression<Func<TClass, TProp>> memberExpression, string defaultValue)
        {
            string value = JsonPropertyEvaluator.GetValue(memberExpression);
            return !string.IsNullOrWhiteSpace(value)
                ? value
                : defaultValue;
        }
        public static string GetJsonValue<TClass, TProp>(TClass obj, Expression<Func<TClass, TProp>> memberExpression, string defaultValue)
        {
            return GetJsonValue(memberExpression, defaultValue);
        }
        public static string GetLdapValue(MemberInfo memberInfo)
        {
            return LdapEvaluator.GetValue(memberInfo);
        }
        public static string GetLdapValue<T, TProp>(Expression<Func<T, TProp>> expression)
        {
            return LdapEvaluator.GetValue(expression);
        }

        public static bool InheritsIEnumerable<T>(MemberInfo memberInfo)
        {
            //string mustEqual = Strings.IEnumerableT_Format.Format(typeof(T).ToString());
            Type[] iEnumTypes = memberInfo.ReflectedType.FindInterfaces(new TypeFilter(InterfaceFilter),
                typeof(IEnumerable<T>));

            return null != iEnumTypes && iEnumTypes.Length > 0;
        }

        private static bool InterfaceFilter(Type typeObj, object criteriaObj)
        {
            return typeObj.ToString().Equals(criteriaObj?.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
