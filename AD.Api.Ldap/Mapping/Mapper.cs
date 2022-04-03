using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Converters;
using AD.Api.Ldap.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;

namespace AD.Api.Ldap.Mapping
{
    public static partial class Mapper
    {
        public static T? MapFromLiveObject<T>([NotNullIfNotNull("objToMap")] T? objToMap) where T : LiveLdapObject
        {
            if (objToMap is null)
                return default;

            return Reflect(objToMap, objToMap.DirEntry.Properties);
        }

        public static T MapFromDirectoryEntry<T>(DirectoryEntry directoryEntry) where T : new()
        {
            return MapFromDirectoryEntry(new T(), directoryEntry);
        }

        [return: NotNullIfNotNull("objToMap")]
        public static T? MapFromDirectoryEntry<T>(T? objToMap, DirectoryEntry directoryEntry)
        {
            return objToMap is not null
                ? Reflect(objToMap, directoryEntry.Properties)
                : default;
        }

        [return: NotNull]
        private static T Reflect<T>([DisallowNull] T obj, PropertyCollection collection)
        {
            HashSet<string> namesUsed = new(StringComparer.CurrentCultureIgnoreCase);
            List<(MemberInfo Member, LdapPropertyAttribute Attribute)> bindables = GetBindableMembers<T>();

            bindables.ForEach(bindable =>
            {
                string? key = null;

                if (collection.Contains(bindable.Member.Name))
                    key = bindable.Member.Name;
                
                else if (!string.IsNullOrWhiteSpace(bindable.Attribute.LdapName) && collection.Contains(bindable.Attribute.LdapName))
                    key = bindable.Attribute.LdapName;

                else
                    return;

                object? convertedValue = ConvertValue(obj, bindable.Member, bindable.Attribute, collection[key].Value);

                if (convertedValue is not null)
                {
                    ApplyMemberValue(obj, bindable.Member, convertedValue);
                    _ = namesUsed.Add(key);
                }
            });

            IEnumerable<MemberInfo> extDataMems = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CustomAttributes.Any(att => att.AttributeType.IsAssignableTo(typeof(LdapExtensionDataAttribute))));

            MemberInfo? extDataMem = extDataMems.FirstOrDefault();
            if (extDataMem is not null)
            {
                var dict = new Dictionary<string, object[]?>(collection.Count, StringComparer.CurrentCultureIgnoreCase);

                foreach (string propertyName in collection.PropertyNames.Cast<string>()
                    .Where(x => !namesUsed.Contains(x)))
                {
                    var prop = collection[propertyName];
                    dict.Add(propertyName, prop?.Cast<object>().ToArray());
                }

                if (extDataMem is FieldInfo fi)
                    fi.SetValue(obj, dict);

                else if (extDataMem is PropertyInfo pi)
                    pi.SetValue(obj, dict);
            }

            return obj;
        }
    }
}