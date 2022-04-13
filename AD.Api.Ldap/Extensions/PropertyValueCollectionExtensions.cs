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
        public static bool TryGetPropertyValueCollection(this PropertyCollection? propCol, string? propertyName,
            [NotNullWhen(true)] out PropertyValueCollection? propertyValueCollection)
        {
            propertyValueCollection = null;
            if (propCol is null || string.IsNullOrWhiteSpace(propertyName))
                return false;

            propertyValueCollection = propCol[propertyName];

            return propertyValueCollection is not null;
        }

        public static bool TryGetFirstValue<T>(this PropertyCollection? propCol, string? propertyName,
            [NotNullWhen(true)] out T? value)
        {
            return TryGetValue(propCol, propertyName, out value, x => x.Cast<object>().FirstOrDefault());
        }

        public static bool TryGetLastValue<T>(this PropertyCollection? propCol, string? propertyName,
            [NotNullWhen(true)] out T? value)
        {
            return TryGetValue(propCol, propertyName, out value, x => x.Cast<object>().LastOrDefault());
        }

        public static bool TryGetValue<T>(this PropertyCollection? propCol, string? propertyName,
            [NotNullWhen(true)] out T? value,
            Func<PropertyValueCollection, object?> valueSelector)
        {
            value = default;
            if (propCol is null || string.IsNullOrWhiteSpace(propertyName))
                return false;

            PropertyValueCollection? propValCol = propCol[propertyName];
            object? firstValue = propValCol?.Count > 1
                ? valueSelector(propValCol)
                : (propValCol?[0]);

            if (firstValue is T tVal)
            {
                value = tVal;
                return true;
            }
            else if (firstValue is IConvertible icon)
            {
                try
                {
                    value = (T)Convert.ChangeType(icon, typeof(T));
                    return value is not null;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public static bool TryGetValues<T>(this PropertyCollection? propCol, string? propertyName,
            out T[] values)
        {
            values = Array.Empty<T>();
            if (propCol is null || string.IsNullOrWhiteSpace(propertyName))
                return false;

            PropertyValueCollection? propValCol = propCol[propertyName];
            if (propValCol is null)
                return false;

            values = new T[propValCol.Count];
            Type convertToType = GetNullTypeOrDefault(typeof(T));

            int count = 0;
            for (int i = 0; i < propValCol.Count; i++)
            {
                object? value = propValCol[i];
                if (value is T tVal)
                {
                    values[i] = tVal;
                    count++;
                    continue;
                }
                else if (convertToType.IsAssignableTo(typeof(IConvertible))
                    && value is IConvertible icon)
                {
                    try
                    {
                        values[i] = (T)Convert.ChangeType(icon, convertToType);
                        count++;
                        continue;
                    }
                    catch
                    {
                        break;  // Don't even bother going through the rest.
                    }
                }
            }

            if (count < propValCol.Count)
                Array.Resize(ref values, count);

            return count > 0;
        }

        private static Type GetNullTypeOrDefault(Type possibleNullable)
        {
            Type? underlying = Nullable.GetUnderlyingType(possibleNullable);

            return underlying is null
                ? possibleNullable
                : underlying;
        }
    }
}
