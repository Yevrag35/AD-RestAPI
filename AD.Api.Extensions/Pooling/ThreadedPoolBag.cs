using AD.Api.Actions;
using AD.Api.Pooling;
using ConcurrentCollections;
using System.Collections;
using System.Collections.Concurrent;

namespace AD.Api.Core.Pooling
{
    public abstract class ThreadedPoolBag<T> : IPoolReturner<T> where T : class
    {
        private const int DEFAULT_MAX_SIZE = 10;

        private readonly ConcurrentBag<T> _bag;
        private readonly ConcurrentHashSet<Guid> _leasedIds;

        private readonly IStatedCallback<T> _callback;

        protected bool IsEmpty => _bag.IsEmpty;
        protected IReadOnlyCollection<Guid> LeasedIds => _leasedIds;
        public int MaxSize { get; protected set; } = DEFAULT_MAX_SIZE;
        protected IServiceProvider Services { get; }

        protected ThreadedPoolBag(IServiceProvider provider)
            : this(null, provider, GetCallbackOrDefault(provider))
        {
        }
        protected ThreadedPoolBag(IServiceProvider provider, Func<IServiceProvider, T> implementationFactory)
            : this(null, provider, StatedCallback.Create(provider, implementationFactory))
        {
        }
        protected ThreadedPoolBag(IEnumerable<T>? collection, IServiceProvider provider, IStatedCallback<T> callback)
        {
            _bag = collection is null
                ? []
                : new(collection);

            _leasedIds = new(Environment.ProcessorCount, _bag.IsEmpty ? DEFAULT_MAX_SIZE / 2 : _bag.Count);
            this.Services = provider;
            _callback = callback;
        }

        [DebuggerStepThrough]
        private static T DefaultFactory(IServiceProvider provider)
        {
            return provider.GetRequiredService<T>();
        }
        private static IStatedCallback<T> GetCallbackOrDefault(IServiceProvider provider)
        {
            IStatedCallback<T>? callback = provider.GetService<IStatedCallback<T>>();
            return callback ?? StatedCallback.Create(provider, DefaultFactory);
        }

        protected Guid GenerateLease()
        {
            Guid id = Guid.NewGuid();
            while (!_leasedIds.Add(id))
            {
                id = Guid.NewGuid();
            }

            return id;
        }
        protected T GetOrConstruct(out bool constructed)
        {
            constructed = false;
            if (!_bag.TryTake(out T? item))
            {
                item = _callback.Invoke();
                constructed = true;
            }

            return item;
        }

        protected static IPooledItem<T> GetPooledItem<TPool>(TPool pool, Func<TPool, T> getItemFunc) where TPool : ThreadedPoolBag<T>
        {
            var item = getItemFunc(pool);
            Guid lease = pool.GenerateLease();

            return new PooledItem<T, TPool>(in lease, item, pool);
        }

        protected abstract bool Reset([DisallowNull] T item);
        public void Return(T? item)
        {
            if (item is null)
            {
                Debug.Fail("Tried to return a NULL item back to the pool.");
                return;
            }

            this.ReturnCore(Guid.Empty, item);
        }
        public void Return(Guid lease, T? item)
        {
            if (item is null)
            {
                Debug.Fail("Tried to return a NULL item back to the pool.");
                return;
            }

            this.ReturnCore(lease, item);
        }
        protected virtual void ReturnCore(Guid lease, [DisallowNull] T item)
        {
            if (lease != Guid.Empty && !this.ReturnLease(lease))
            {
                return;
            }
            else if (this.Reset(item))
            {
                _ = this.TryReturn(item);
            }
        }
        private bool ReturnLease(Guid lease)
        {
            return _leasedIds.TryRemove(lease);
        }
        private bool TryReturn([DisallowNull] T item)
        {
            if (_bag.IsEmpty || this.MaxSize > _bag.Count)
            {
                _bag.Add(item);
                return true;
            }

            return false;
        }
    }
}
