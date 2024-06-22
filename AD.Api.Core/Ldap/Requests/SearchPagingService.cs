using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Requests.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace AD.Api.Core.Ldap
{
    public interface ISearchPagingService
    {
        Guid AddRequest(SearchParameters requestToCopy, byte[] continueKey);
        bool TryGetRequest(Guid cacheKey, HttpContext httpContext, [NotNullWhen(true)] out CachedSearchParameters? request);
    }

    [DependencyRegistration(typeof(ISearchPagingService), Lifetime = ServiceLifetime.Singleton)]
    public sealed class SearchPagingService : ISearchPagingService
    {
        private readonly IMemoryCache _cache;

        public SearchPagingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Guid AddRequest(SearchParameters requestToCopy, byte[] continueKey)
        {
            if (requestToCopy is CachedSearchParameters cached)
            {
                _cache.Remove(cached.ContinueKey);
                Guid nextKey = Guid.NewGuid();
                cached.ContinueKey = nextKey;
                cached.CookieBytes = continueKey;

                cached.Dissipate();
                _cache.Set(cached.ContinueKey, cached, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    Priority = CacheItemPriority.Low,
                    Size = 100L,
                });

                return nextKey;
            }

            Guid cacheKey = Guid.NewGuid();
            cached = new(requestToCopy, continueKey);
            requestToCopy.Dissipate();

            _cache.Set(cacheKey, cached, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Low,
                Size = 100L,
            });

            return cacheKey;
        }

        public bool TryGetRequest(Guid cacheKey, HttpContext httpContext, [NotNullWhen(true)] out CachedSearchParameters? request)
        {
            if (_cache.TryGetValue(cacheKey, out request) && request is not null)
            {
                request.Rehydrate(httpContext);
                return true;
            }
            else
            {
                request = null;
                return false;
            }
        }
    }
}

