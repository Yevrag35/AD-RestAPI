namespace AD.Api.Pooling
{
    /// <summary>
    /// An interface allowing for returning items to a pool.
    /// </summary>
    /// <typeparam name="T">The type of object being returned.</typeparam>
    public interface IPoolReturner<T> : IPoolLeaseReturner<T> where T : class
    {
        /// <summary>
        /// The maximum number of items that can be stored in the pool.
        /// </summary>
        int MaxSize { get; }

        /// <summary>
        /// Returns and resets the specified item to the pool.
        /// </summary>
        /// <param name="item">The object being returned.</param>
        void Return(T? item);
    }
}
