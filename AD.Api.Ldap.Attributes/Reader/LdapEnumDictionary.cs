using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AD.Api.Ldap.Attributes
{
    public interface ILdapEnumDictionary : IReadOnlyDictionary<string, Type>
    {
        bool TryGetIntValue(string ldapName, string? strValue, out int result);
        bool TryGetEnumValue(string ldapName, string strValue, [NotNullWhen(true)] out Enum? results);
    }

    public static class EnumReader
    {
        public static ILdapEnumDictionary GetLdapEnums(Assembly[] assemblies)
        {
            Type enumAttType = typeof(LdapEnumAttribute);
            var dict = new LdapEnumDictionary(10);

            Array.ForEach(assemblies, ass =>
            {
                Type[] loadedTypes = ass.GetExportedTypes();

                Array.ForEach(loadedTypes, type =>
                {
                    if (type.IsEnum && type.CustomAttributes.Any(x => enumAttType.IsAssignableFrom(x.AttributeType)))
                    {
                        LdapEnumAttribute att = type.GetCustomAttribute<LdapEnumAttribute>()
                            ?? throw new Exception("WTF???");

                        string? name = att.Name;

                        if (string.IsNullOrWhiteSpace(name))
                            name = type.Name.ToLower();

                        if (!dict.ContainsKey(name))
                            dict.Add(name, type);
                    }
                });

                Array.Clear(loadedTypes, 0, loadedTypes.Length);
            });

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
