using System.Collections;

namespace AD.Api.Collections
{
    public static class DictionaryExtensions
    {
        public static void AddMany<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Span<TKey> keys, TValue value)where TKey : notnull
        {
            foreach (TKey key in keys)
            {
                dictionary.Add(key, value);
            }
        }

        public static bool TryGetValue<T>(this IDictionary dictionary, object key, [NotNullWhen(true)] out T? value)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(key);

            if (!dictionary.Contains(key))
            {
                value = default;
                return false;
            }

            object? o = dictionary[key];
            if (o is T tVal)
            {
                value = tVal;
                return true;
            }
            else if (o is ICollection iCol && iCol.TryGetFirst(out value))
            {
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
        public static bool TryGetValues<T>(this IDictionary dictionary, object key, out T[] values)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(key);

            if (!dictionary.Contains(key))
            {
                values = [];
                return false;
            }

            object? o = dictionary[key];

            if (o is T tVal)
            {
                values = [tVal];
                return true;
            }
            else if (o is ICollection iCol && iCol.TryGetAll(out values))
            {
                return true;
            }

            values = [];
            return false;
        }
    }
}

