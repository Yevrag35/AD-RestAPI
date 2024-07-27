using AD.Api.Attributes.Services;
using AD.Api.Core.Pooling;
using System.ComponentModel;

namespace AD.Api.Pooling
{
    [DynamicDependencyRegistration]
    public sealed class StringHashSetPool : ThreadedPoolBag<HashSet<string>>, IPoolBagLeaser<HashSet<string>>
    {
        private const int MAXIMUM_CAPACITY = 5000;
        private const int MINIMUM_CAPACITY = 50;

        public StringHashSetPool(IServiceProvider provider)
            : base(provider, CreateInstance)
        {
        }

        private static HashSet<string> CreateInstance(IServiceProvider _)
        {
            return new HashSet<string>(MINIMUM_CAPACITY, StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> Get()
        {
            return this.GetOrConstruct(out _);
        }
        public IPooledItem<HashSet<string>> GetPooledItem()
        {
            return GetPooledItem(this, pool => pool.Get());
        }

        protected override bool Reset([DisallowNull] HashSet<string> item)
        {
            int capacity = item.EnsureCapacity(MINIMUM_CAPACITY);
            item.Clear();
            if (capacity > MAXIMUM_CAPACITY)
            {
                item.TrimExcess();
                capacity = item.EnsureCapacity(MINIMUM_CAPACITY);
                Debug.Assert(capacity <= MAXIMUM_CAPACITY);
            }

            return true;
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            static StringHashSetPool getPool(IServiceProvider x) => x.GetRequiredService<StringHashSetPool>();

            services.AddSingleton<StringHashSetPool>()
                    .AddSingleton<IPoolBagLeaser<HashSet<string>>>(getPool)
                    .AddSingleton<IPoolBag<HashSet<string>>>(getPool)
                    .AddSingleton<IPoolLeaseReturner<HashSet<string>>>(getPool)
                    .AddSingleton<IPoolReturner<HashSet<string>>>(getPool)
                    .AddScoped(x => getPool(x).GetPooledItem());
        }
    }
}
