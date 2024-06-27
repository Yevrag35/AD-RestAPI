using AD.Api.Attributes.Services;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Runtime.Versioning;

namespace AD.Api.Core.Security
{
    [SupportedOSPlatform("WINDOWS")]
    public interface ISidResolutionService : IRestrictedSids
    {
        SidString GetOrAdd(string securityIdentifier);
    }

    [DynamicDependencyRegistration]
    [SupportedOSPlatform("WINDOWS")]
    internal class SidResolutionService : ISidResolutionService
    {
        private static readonly TimeSpan DEFAULT_EXPIRATION = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;
        private readonly IRestrictedSids _restrictedSids;

        public SidResolutionService(IRestrictedSids restricted, IMemoryCache cache)
        {
            _cache = cache;
            _restrictedSids = restricted;
        }

        public bool Contains(string securityIdentifier)
        {
            return _cache.Get(securityIdentifier) is null && _restrictedSids.Contains(securityIdentifier);
        }
        public SidString GetOrAdd(string securityIdentifier)
        {
            if (!_cache.TryGetValue(securityIdentifier, out SidString? sid) || sid is null)
            {
                sid = _cache.Set(securityIdentifier, new SidString(securityIdentifier), new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = DEFAULT_EXPIRATION,
                    Priority = CacheItemPriority.Low,
                    Size = 5L,
                });
            }

            return sid;
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton<ISidResolutionService>(x =>
            {
                RestrictedSids sids = x.GetRequiredService<RestrictedSids>();
                IMemoryCache cache = x.GetRequiredService<IMemoryCache>();
                return new SidResolutionService(sids, cache);
            })
                .AddSingleton<IRestrictedSids>(x => x.GetRequiredService<ISidResolutionService>());
        }
    }
}

