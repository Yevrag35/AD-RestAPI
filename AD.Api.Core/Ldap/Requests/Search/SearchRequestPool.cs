using AD.Api.Attributes.Services;
using AD.Api.Core.Pooling;
using AD.Api.Pooling;
using System.ComponentModel;

namespace AD.Api.Core.Ldap.Requests.Search
{
    [DynamicDependencyRegistration]
    public sealed class SearchRequestPool : ThreadedPoolBag<LdapSearchRequest>, IPoolBagLeaser<LdapSearchRequest>
    {
        public SearchRequestPool(IServiceProvider provider)
            : base(provider)
        {
        }
        public LdapSearchRequest Get()
        {
            LdapSearchRequest request = this.GetOrConstruct(out _);
            Guid requestId = this.GenerateLease();
            request.RequestId = requestId;
            return request;
        }
        public IPooledItem<LdapSearchRequest> GetPooledItem()
        {
            LdapSearchRequest request = this.Get();
            return new RequestPooledItem(this, request);
        }
        protected override bool Reset([DisallowNull] LdapSearchRequest item)
        {
            item.Reset();
            return true;
        }
        protected override void ReturnCore(Guid lease, [DisallowNull] LdapSearchRequest item)
        {
            if (lease == Guid.Empty)
            {
                lease = item.RequestId;
                if (lease == Guid.Empty)
                {
                    Debug.Fail("Tried to return a request without a lease ID.");
                    return;
                }
            }

            base.ReturnCore(lease, item);
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            static SearchRequestPool func(IServiceProvider x) => x.GetRequiredService<SearchRequestPool>();

            services.AddSingleton<SearchRequestPool>()
                    .AddSingleton<IPoolBagLeaser<LdapSearchRequest>>(func)
                    .AddSingleton<IPoolBag<LdapSearchRequest>>(func)
                    .AddSingleton<IPoolLeaseReturner<LdapSearchRequest>>(func)
                    .AddSingleton<IPoolReturner<LdapSearchRequest>>(func)
                    .AddScoped(x =>
                    {
                        return func(x).GetPooledItem();
                    });
        }

        private readonly struct RequestPooledItem : IPooledItem<LdapSearchRequest>
        {
            private readonly SearchRequestPool _pool;
            private readonly LdapSearchRequest _value;
            public readonly Guid LeaseId => this.Value.RequestId;
            public readonly LdapSearchRequest Value => _value;

            internal RequestPooledItem(SearchRequestPool pool, LdapSearchRequest request)
            {
                _value = request;
                _pool = pool;
            }

            public void Dispose()
            {
                _pool.Return(_value.RequestId, _value);
            }
        }
    }
}

