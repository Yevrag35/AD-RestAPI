namespace AD.Api.Extensions.Collections
{
    public static class ListExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <exception cref="ArgumentNullException"/>
        public static void AddRange<T>(this IList<T> collection, params T[] items)
        {
            ArgumentNullException.ThrowIfNull(collection);
            if (items is null)
            {
                return;
            }

            foreach (T item in items)
            {
               collection.Add(item);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        /// <exception cref="ArgumentNullException"/>
        public static void AddRange<T>(this IList<T> collection, ReadOnlySpan<T> items)
        {
            ArgumentNullException.ThrowIfNull(collection);
            if (items.IsEmpty)
            {
                return;
            }

            foreach (T item in items)
            {
                collection.Add(item);
            }
        }
    }
}