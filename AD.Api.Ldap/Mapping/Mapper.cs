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
        public static T? MapFromDirectoryEntry<T>([NotNullIfNotNull("objToMap")] T? objToMap, DirectoryEntry directoryEntry)
        {
            if (objToMap is null)
                return default;

            return Reflect(objToMap, directoryEntry.Properties);
        }

        private static T Reflect<T>([DisallowNull] T obj, PropertyCollection collection)
        {
            List<(MemberInfo Member, LdapPropertyAttribute Attribute)> bindables = GetBindableMembers<T>();

            bindables.ForEach(bindable =>
            {
                string? key = null;
                if (collection.Contains(bindable.Member.Name))
                {
                    key = bindable.Member.Name;
                }
                else if (collection.Contains(bindable.Attribute.LdapName))
                    key = bindable.Attribute.LdapName;

                else
                    return;

                object? convertedValue = ConvertValue(obj, bindable.Member, bindable.Attribute, collection[key].Value);
                if (convertedValue is not null)
                    ApplyMemberValue(obj, bindable.Member, convertedValue);
            });

            return obj;
        }
    }
}