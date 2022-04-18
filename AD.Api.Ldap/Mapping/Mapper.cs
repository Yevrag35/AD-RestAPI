using AD.Api.Ldap.Attributes;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Reflection;

namespace AD.Api.Ldap.Mapping
{
    public static partial class Mapper
    {
        [Obsolete]
        public static T MapFromDirectoryEntry<T>(DirectoryEntry directoryEntry, ILdapEnumDictionary enumDictionary) where T : new()
        {
            return MapFromDirectoryEntry(new T(), directoryEntry, enumDictionary);
        }
        public static T MapFromSearchResult<T>(SearchResult searchResult, ILdapEnumDictionary enumDictionary) where T : new()
        {
            return MapFromSearchResult(new T(), searchResult, enumDictionary);
        }

        [return: NotNullIfNotNull("objToMap")]
        public static T? MapFromDirectoryEntry<T>(T? objToMap, DirectoryEntry directoryEntry, ILdapEnumDictionary enumDictionary)
        {
            return objToMap is not null
                ? Reflect(objToMap, directoryEntry.Properties, enumDictionary)
                : default;
        }

        [return: NotNullIfNotNull("objToMap")]
        public static T? MapFromSearchResult<T>(T? objToMap, SearchResult searchResult, ILdapEnumDictionary enumDictionary)
        {
            return objToMap is not null
                ? Reflect(objToMap, searchResult.Properties, enumDictionary)
                : default;
        }

        [return: NotNull]
        private static T Reflect<T>([DisallowNull] T obj, IDictionary collection, ILdapEnumDictionary enumDictionary)
        {
            HashSet<string> namesUsed = new(StringComparer.CurrentCultureIgnoreCase);
            List<(MemberInfo Member, LdapPropertyAttribute Attribute)> bindables = GetBindableMembers<T>();

            //bindables.ForEach(bindable =>
            for (int i = 0; i < bindables.Count; i++)
            {
                var bindable = bindables[i];

                string? key = null;

                if (collection.Contains(bindable.Member.Name.ToLower()))
                    key = bindable.Member.Name.ToLower();

                else if (!string.IsNullOrWhiteSpace(bindable.Attribute.LdapName) && collection.Contains(bindable.Attribute.LdapName))
                    key = bindable.Attribute.LdapName;

                else
                    continue;

                object? convertedValue = ConvertValue(obj, bindable.Member, bindable.Attribute, collection[key] as IEnumerable);

                if (convertedValue is not null)
                {
                    ApplyMemberValue(obj, bindable.Member, convertedValue);
                    _ = namesUsed.Add(key);
                }
            }

            IEnumerable<MemberInfo> extDataMems = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CustomAttributes.Any(att => att.AttributeType.IsAssignableTo(typeof(LdapExtensionDataAttribute))));

            MemberInfo? extDataMem = extDataMems.FirstOrDefault();
            LdapExtensionDataAttribute? att = extDataMem?.GetCustomAttribute<LdapExtensionDataAttribute>();

            if (extDataMem is not null && att is not null)
            {
                var dict = new SortedDictionary<string, object[]?>(StringComparer.CurrentCultureIgnoreCase);

                foreach (string propertyName in collection.Keys.Cast<string>()
                    .Where(x => !namesUsed.Contains(x)))
                {
                    IEnumerable<object>? enumerable = (collection[propertyName] as IEnumerable)?.Cast<object>();

                    if (enumDictionary.TryGetValue(propertyName, out Type? enumType) && 
                        enumerable is not null)
                    {
                        object? firstVal = enumerable?.FirstOrDefault();

                        if (firstVal is not long enumVal)
                        {
                            if (firstVal is int intVal)
                                enumVal = (long)intVal;

                            else
                                continue;
                        }

                        object? realValue = ConvertEnum(enumVal, enumType);
                        if (realValue is not null)
                            dict.Add(propertyName, new object[] { realValue });

                        continue;
                    }

                    if (enumerable is null)
                    {
                        dict.Add(propertyName, Array.Empty<object>());
                        continue;
                    }

                    if (!att.IncludeComObjects
                        && 
                        enumerable.OfType<MarshalByRefObject>().Any())
                    {
                        continue;
                    }

                    object[] prop = enumerable.ToArray();

                    dict.Add(propertyName, prop);
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