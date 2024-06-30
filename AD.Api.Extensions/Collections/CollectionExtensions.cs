using System.Collections;

namespace AD.Api.Collections
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsFixedSize<T>(this ICollection<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);
            return collection.IsReadOnly || collection is Array || IsFixedSizeList(collection);
        }
        private static bool IsFixedSizeList<T>(this ICollection<T> collection)
        {
            return collection is IList nonGenList && nonGenList.IsFixedSize;
        }

        public static bool TryGetFirst<T>(this ICollection collection, [NotNullWhen(true)] out T? value)
        {
            foreach (object? o in collection)
            {
                value = (T?)o;
                return value is not null;
            }

            value = default;
            return false;
        }
        public static bool TryGetAll<T>(this ICollection collection, out T[] values)
        {
            if (collection.Count <= 0)
            {
                values = [];
                return false;
            }

            values = new T[collection.Count];
            int i = 0;
            foreach (object? o in collection)
            {
                if (o is T tVal)
                {
                    values[i++] = tVal;
                }
            }

            if (i == collection.Count)
            {
                return true;
            }
            else if (i == 0)
            {
                values = [];
                return false;
            }
            else
            {
                Debug.Fail("Some values were not of the expected type.");
                Array.Resize(ref values, i);
                return true;
            }
        }
    }
}

