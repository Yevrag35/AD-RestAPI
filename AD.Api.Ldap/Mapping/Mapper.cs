using AD.Api.Ldap.Attributes;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Reflection;

namespace AD.Api.Ldap.Mapping
{
    public static partial class Mapper
    {
        public static T MapFromDirectoryEntry<T>(DirectoryEntry directoryEntry) where T : new()
        {
            return MapFromDirectoryEntry(new T(), directoryEntry);
        }
        public static T MapFromSearchResult<T>(SearchResult searchResult) where T : new()
        {
            return MapFromSearchResult(new T(), searchResult);
        }

        [return: NotNullIfNotNull("objToMap")]
        public static T? MapFromDirectoryEntry<T>(T? objToMap, DirectoryEntry directoryEntry)
        {
            return objToMap is not null
                ? Reflect(objToMap, directoryEntry.Properties)
                : default;
        }

        [return: NotNullIfNotNull("objToMap")]
        public static T? MapFromSearchResult<T>(T? objToMap, SearchResult searchResult)
        {
            return objToMap is not null
                ? Reflect(objToMap, searchResult.Properties)
                : default;
        }

        [return: NotNull]
        private static T Reflect<T>([DisallowNull] T obj, IDictionary collection)
        {
            HashSet<string> namesUsed = new(StringComparer.CurrentCultureIgnoreCase);
            List<(MemberInfo Member, LdapPropertyAttribute Attribute)> bindables = GetBindableMembers<T>();

            bindables.ForEach(bindable =>
            {
                string? key = null;

                if (collection.Contains(bindable.Member.Name.ToLower()))
                    key = bindable.Member.Name.ToLower();
                
                else if (!string.IsNullOrWhiteSpace(bindable.Attribute.LdapName) && collection.Contains(bindable.Attribute.LdapName))
                    key = bindable.Attribute.LdapName;

                else
                    return;

                object? convertedValue = ConvertValue(obj, bindable.Member, bindable.Attribute, collection[key] as IEnumerable);

                if (convertedValue is not null)
                {
                    ApplyMemberValue(obj, bindable.Member, convertedValue);
                    _ = namesUsed.Add(key);
                }
            });

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