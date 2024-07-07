using AD.Api.Extensions.Comparisons;
using System.Collections;
using System.Collections.Concurrent;

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

        [DebuggerStepThrough]
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
        {
            return ToConcurrentDictionary(source, Environment.ProcessorCount, keyComparer);
        }
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, int concurrencyLevel, IEqualityComparer<TKey>? keyComparer = null) where TKey : notnull
        {
            if (source is ConcurrentDictionary<TKey, TValue> alreadyConcurrent)
            {
                return ShouldUseExisting(in concurrencyLevel, keyComparer, alreadyConcurrent)
                    ? alreadyConcurrent
                    : new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, source, keyComparer);
            }

            return new(concurrencyLevel, source, keyComparer);
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
        public static bool TryGetValue<TKey, TOther, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TOther? initialize, [MaybeNullWhen(false)] out TValue value) where TKey : notnull
        {
            initialize = default;
            return dictionary.TryGetValue(key, out value);
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

        private static bool ComparerSpecifiedAndMatches<TKey>([NotNullWhen(true)] IEqualityComparer<TKey>? specified, IEqualityComparer<TKey> existing) where TKey : notnull
        {
            if (existing.RefEqualsOrNull(specified, out bool result))
            {
                return result;
            }

            return existing.GetType().Equals(specified.GetType());
        }
        private static bool ShouldUseExisting<TKey, TValue>(in int concurrencyLevel, IEqualityComparer<TKey>? keyComparer, ConcurrentDictionary<TKey, TValue> existing) where TKey : notnull
        {
            return concurrencyLevel == Environment.ProcessorCount
                && ComparerSpecifiedAndMatches(keyComparer, existing.Comparer);
        }
    }
}

