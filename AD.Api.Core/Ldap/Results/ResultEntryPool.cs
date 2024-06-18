using AD.Api.Actions;
using AD.Api.Attributes.Services;
using AD.Api.Core.Pooling;
using AD.Api.Core.Schema;
using AD.Api.Core.Serialization;
using AD.Api.Pooling;
using ConcurrentCollections;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Concurrent;

namespace AD.Api.Core.Ldap.Results
{
    [DynamicDependencyRegistration]
    public sealed class ResultEntryPool : IPoolReturner<ResultEntry>, IPoolReturner<ResultEntryCollection>
    {
        const int MAX_COL_SIZE = 10; const int MAX_ENT_SIZE = 200;

        private readonly ConcurrentBag<ResultEntryCollection> _collections;
        private readonly IStatedCallback<ResultEntry> _createEntry;
        private readonly IStatedCallback<ResultEntryCollection> _createCollection;
        private readonly ConcurrentBag<ResultEntry> _entries;
        private readonly ConcurrentHashSet<Guid> _leaseIds;
        
        private int _colCount; int _entCount;

        int IPoolReturner<ResultEntry>.MaxSize => MAX_ENT_SIZE;
        int IPoolReturner<ResultEntryCollection>.MaxSize => MAX_COL_SIZE;

        public ResultEntryPool(ISchemaService schemaSvc, IAttributeConverter converter)
        {
            _collections = [];
            _entries = [];
            _leaseIds = new(Environment.ProcessorCount, MAX_COL_SIZE);
            _createEntry = StatedCallback.Create(converter, x => new ResultEntry(x));
            _createCollection = StatedCallback.Create(this, x => new ResultEntryCollection(MAX_COL_SIZE, x));
        }

        public ResultEntry GetItem()
        {
            ResultEntry entry = TakeOrCreate(_entries, ref _entCount, _createEntry);

            entry.LeaseId = this.GenerateLease();
            return entry;
        }

        public IPooledItem<ResultEntry> GetPooledItem()
        {
            ResultEntry item = this.GetItem();
            return new PooledItem<ResultEntry, ResultEntryPool>(item.LeaseId, item, this);
        }
        public IPooledItem<ResultEntryCollection> GetPooledCollection()
        {
            ResultEntryCollection collection = TakeOrCreate(_collections, ref _colCount, _createCollection);

            Guid lease = this.GenerateLease();
            collection.LeaseId = lease;
            return new PooledItem<ResultEntryCollection, ResultEntryPool>(in lease, collection, this);
        }

        public void Return(ResultEntry? item)
        {
            if (!TryReset(item) || !_leaseIds.TryRemove(item.LeaseId))
            {
                return;
            }

            Return(item, _entries, ref _entCount, MAX_ENT_SIZE);
        }
        void IPoolLeaseReturner<ResultEntry>.Return(Guid itemId, ResultEntry? item)
        {
            if (item is null)
            {
                _leaseIds.TryRemove(itemId);
                return;
            }
            else if (itemId == item.LeaseId && !_leaseIds.TryRemove(itemId))
            {
                return;
            }
            else if (itemId != item.LeaseId && !_leaseIds.TryRemove(itemId) && !_leaseIds.TryRemove(item.LeaseId))
            {
                return;
            }

            Return(item, _entries, ref _entCount, MAX_ENT_SIZE);
        }
        public void Return(Guid itemId, ResultEntryCollection? item)
        {
            if (_leaseIds.TryRemove(itemId) && TryReset(item))
            {
                Return(item, _collections, ref _colCount, MAX_COL_SIZE);
            }
        }
        public void Return(ResultEntryCollection? collection)
        {
            if (TryReset(collection) && _leaseIds.TryRemove(collection.LeaseId))
            {
                Return(collection, _collections, ref _colCount, MAX_COL_SIZE);
            }
        }

        private Guid GenerateLease()
        {
            Guid id = Guid.NewGuid();
            while (!_leaseIds.Add(id))
            {
                id = Guid.NewGuid();
            }

            return id;
        }
        private static void Return<T>([DisallowNull] T item, ConcurrentBag<T> bag, ref int count, [ConstantExpected] int maxBagSize) where T : class
        {
            if (count < maxBagSize)
            {
                count++;
                bag.Add(item);
            }
        }
        private static T TakeOrCreate<T>(ConcurrentBag<T> bag, ref int count, IStatedCallback<T> createOnMiss)
        {
            if (!bag.TryTake(out T? item))
            {
                item = createOnMiss.Invoke();
            }
            else
            {
                count--;
            }

            return item;
        }
        private static bool TryReset<T>([NotNullWhen(true)] T? item) where T : class, IResettable
        {
            return item is not null && item.TryReset();
        }
        
        [DynamicDependencyRegistrationMethod]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton<ResultEntryPool>()
                    .AddScoped(x =>
                    {
                        return x.GetRequiredService<ResultEntryPool>().GetPooledItem();
                    })
                    .AddScoped(x =>
                    {
                        return x.GetRequiredService<ResultEntryPool>().GetPooledCollection();
                    });
        }
    }
}

