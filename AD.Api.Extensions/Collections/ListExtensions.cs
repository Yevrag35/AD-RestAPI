namespace AD.Api.Extensions.Collections
{
    public static class ListExtensions
    {
        public static void AddRange<T>(this IList<T> collection, params T[] items)
        {
            if (items is null)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                collection.Add(items[i]);
            }
        }
    }
}