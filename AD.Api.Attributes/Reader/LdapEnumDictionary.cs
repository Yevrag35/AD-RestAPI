using System.Collections;

namespace AD.Api.Ldap.Attributes
{
    [Obsolete("Use 'IEnumValues' instead.")]
    public interface ILdapEnumDictionary : IReadOnlyDictionary<string, Type>, ICollection
    {
        bool TryGetIntValue(string ldapName, string? strValue, out int result);
        bool TryGetEnumValue(string ldapName, string strValue, [NotNullWhen(true)] out Enum? results);
    }

    [Obsolete("Use 'IEnumValues' instead.")]
    public static class EnumReader
    {
        private static readonly Action<Type, Type, LdapEnumDictionary> _typeAction = (input, enumAttType, dict) =>
        {
            if (input.IsEnum && input.CustomAttributes.Any(x => enumAttType.IsAssignableFrom(x.AttributeType)))
            {
                LdapEnumAttribute att = input.GetCustomAttribute<LdapEnumAttribute>()
                    ?? throw new Exception("WTF???");

                string name = att.Name ?? string.Empty; // not sure why 'String.Empty' was needed for CodeAnalysis -_-

                if (string.IsNullOrWhiteSpace(name))
                    name = input.Name.ToLower();

                if (!dict.ContainsKey(name))
                    dict.Add(name, input);
            }
        };
        private static readonly Action<Assembly, Type, LdapEnumDictionary, Action<Type, Type, LdapEnumDictionary>> _assAction = (ass, enumAttType, dict, action) =>
        {
            Type[] loadedTypes = ass.GetExportedTypes();
            for (int i = 0; i < loadedTypes.Length; i++)
            {
                action(loadedTypes[i], enumAttType, dict);
            }

            Array.Clear(loadedTypes, 0, loadedTypes.Length);
        };

        public static ILdapEnumDictionary GetLdapEnums(Assembly[] assemblies)
        {
            Type enumAttType = typeof(LdapEnumAttribute);
            var dict = new LdapEnumDictionary(10);

            for (int i = 0; i < assemblies.Length; i++)
            {
                _assAction(assemblies[i], enumAttType, dict, _typeAction);
            }

            return dict;
        }

        private class LdapEnumDictionary : Dictionary<string, Type>, ILdapEnumDictionary
        {
            public LdapEnumDictionary(int capacity = 10)
                : base(capacity, StringComparer.CurrentCultureIgnoreCase)
            {
            }

            public bool TryGetIntValue(string ldapName, string? strValue, out int result)
            {
                result = -1;
                if (this.TryGetEnumValue(ldapName, strValue, out Enum? results))
                {
                    try
                    {
                        result = Convert.ToInt32(results);
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }
            public bool TryGetEnumValue(string ldapName, string? strValue, [NotNullWhen(true)] out Enum? results)
            {
                results = default;
                if (string.IsNullOrWhiteSpace(ldapName))
                    throw new ArgumentNullException(nameof(ldapName));

                if (this.TryGetValue(ldapName, out Type? value))
                {
                    try
                    {
                        results = (Enum)Enum.Parse(value, strValue, true);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                return false;
            }
        }
    }
}
