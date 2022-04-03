using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Extensions
{
    public static class PropertyValueCollectionExtensions
    {
        public static bool TryGetAsEnumerable<T>([NotNullWhen(true)] this PropertyCollection? collection, 
           [NotNullWhen(true)] string? key, [NotNullWhen(true)] out IEnumerable<T>? values)
        {
            values = null;

            if (TryGetPropValue(collection, key, out object? value) && value is IEnumerable enumObj)
            {
                try
                {
                    values = enumObj.Cast<T>();
                    return values is not null;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public static bool TryGetAsSingle<T>([NotNullWhen(true)] this PropertyCollection? collection,
            [NotNullWhen(true)] string? key, [NotNullWhen(true)] out T? value)
        {
            return TryGetAsSingle(collection, key, out value, many => many.FirstOrDefault());
        }

        public static bool TryGetAsSingle<T>([NotNullWhen(true)] this PropertyCollection? collection,
            [NotNullWhen(true)] string? key, [NotNullWhen(true)] out T? value, Func<IEnumerable<T>, T?> selectFunc)
        {
            value = default;
            if (TryGetPropValue(collection, key, out object? propValue))
            {
                if (propValue is string)
                {
                    if (!typeof(T).Equals(typeof(string)) && typeof(T).IsAssignableTo(typeof(IConvertible)))
                    {
                        try
                        {
                            value = (T)Convert.ChangeType(propValue, typeof(T));
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
                else if (propValue is IEnumerable enumObj)
                {
                    try
                    {
                        value = selectFunc(enumObj.Cast<T>());
                        return value is not null;
                    }
                    catch
                    {
                        return false;
                    }
                }

                try
                {
                    value = (T)propValue;
                    return value is not null;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private static bool TryGetPropValue([NotNullWhen(true)] PropertyCollection? collection, [NotNullWhen(true)] string? key,
            [NotNullWhen(true)] out object? value)
        {
            value = null;
            if (collection is null || key is null || !collection.Contains(key))
                return false;

            var propCol = collection[key];
            if (propCol.Count > 0)
            {
                value = propCol.Value;
            }

            return value is not null;
        }
    }
}
