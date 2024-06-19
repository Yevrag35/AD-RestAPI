using AD.Api.Pooling;
using System.Runtime.InteropServices;

namespace AD.Api.Core.Pooling
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct PooledItem<T, TPool> : IPooledItem<T>
        where T : class
        where TPool : class, IPoolLeaseReturner<T>
    {
        private readonly Guid _id;
        private readonly T _item;
        private readonly TPool _pool;

        /// <inheritdoc/>
        public readonly Guid LeaseId => _id;
        /// <inheritdoc/>
        public readonly T Value => _item;

        public PooledItem(in Guid id, [DisallowNull] T item, [DisallowNull] TPool pool)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(pool);

            _pool = pool;
            _id = id;
            _item = item;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _pool.Return(_id, _item);
        }
    }
}

