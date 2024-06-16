using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Services.Schemas;
using AD.Api.Core.Pooling;
using AD.Api.Pooling;
using ConcurrentCollections;
using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Results
{
    [SupportedOSPlatform("WINDOWS")]
    [DynamicDependencyRegistration]
    public sealed class ResultEntryPool : IPoolLeaseReturner<ResultEntry>, IPoolLeaseReturner<ResultEntryCollection>
    {
        const int MAX_COL_SIZE = 10;
        const int MAX_ENT_SIZE = 200;

        private readonly ConcurrentBag<ResultEntryCollection> _collections;
        private readonly PropertyConverter _converter;
        private readonly ConcurrentBag<ResultEntry> _entries;
        private readonly ConcurrentHashSet<Guid> _leaseIds;
        private readonly ISchemaService _schemaSvc;

        public ResultEntryPool(ISchemaService schemaSvc, PropertyConverter converter)
        {
            _collections = [];
            _converter = converter;
            _entries = [];
            _leaseIds = new(Environment.ProcessorCount, 10);
            _schemaSvc = schemaSvc;
        }

        public int MaxSize => throw new NotImplementedException();

        public ResultEntry GetItem()
        {
            if (!_entries.TryTake(out ResultEntry? entry))
            {
                entry = new(_schemaSvc, _converter);
            }

            entry.LeaseId = this.GenerateLease();
            return entry;
        }

        public IPooledItem<ResultEntry> GetPooledItem()
        {
            ResultEntry item = this.GetItem();
            return new PooledItem<ResultEntry, ResultEntryPool>(item.LeaseId, item, this);
        }
        public IPooledItem<ResultEntryCollection> GetPooledCollection(int initialCapacity)
        {
            if (!_collections.TryTake(out ResultEntryCollection? collection))
            {
                collection = new(initialCapacity, this);
            }

            Guid lease = this.GenerateLease();

            return new PooledItem<ResultEntryCollection, ResultEntryPool>(in lease, collection, this);
        }

        public void Return(ResultEntry? item)
        {
            if (item is null || !_leaseIds.TryRemove(item.LeaseId))
            {
                return;
            }

            item.Reset();
            if (_entries.IsEmpty || _entries.Count < MAX_ENT_SIZE)
            {
                item.LeaseId = Guid.Empty;
                _entries.Add(item);
            }
        }
        void IPoolLeaseReturner<ResultEntry>.Return(Guid itemId, ResultEntry? item)
        {
            this.Return(item);
        }
        public void Return(Guid itemId, ResultEntryCollection? item)
        {
            if (item is not null)
            {
                item.ReturnAll();
            }

            if (_leaseIds.TryRemove(itemId) && item is not null && (_collections.IsEmpty || _collections.Count < MAX_COL_SIZE))
            {
                _collections.Add(item);
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
                        return x.GetRequiredService<ResultEntryPool>().GetPooledCollection(10);
                    });
        }
    }
}

