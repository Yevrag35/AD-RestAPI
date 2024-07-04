using AD.Api.Attributes.Services;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Runtime.Versioning;

namespace AD.Api.Core.Security
{
    /// <summary>
    /// Defines a service for resolving and caching SID (Security Identifier) strings.
    /// </summary>
    [SupportedOSPlatform("WINDOWS")]
    public interface ISidResolutionService : IRestrictedSids
    {
        /// <summary>
        /// Gets or adds a <see cref="SidString"/> for the specified security identifier.
        /// </summary>
        /// <param name="securityIdentifier">The security identifier to resolve.</param>
        /// <returns>A <see cref="SidString"/> representing the resolved security identifier.</returns>
        SidString GetOrAdd(string securityIdentifier);
    }

    /// <summary>
    /// Provides a service for resolving and caching SID (Security Identifier) strings.
    /// </summary>
    [DynamicDependencyRegistration]
    [SupportedOSPlatform("WINDOWS")]
    internal class SidResolutionService : ISidResolutionService
    {
        private static readonly TimeSpan DEFAULT_EXPIRATION = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;
        private readonly IRestrictedSids _restrictedSids;

        /// <summary>
        /// Initializes a new instance of the <see cref="SidResolutionService"/> class.
        /// </summary>
        /// <param name="restricted">The restricted SIDs service.</param>
        /// <param name="cache">The memory cache for caching SID strings.</param>
        public SidResolutionService(IRestrictedSids restricted, IMemoryCache cache)
        {
            _cache = cache;
            _restrictedSids = restricted;
        }

        /// <summary>
        /// Determines whether the specified security identifier is restricted.
        /// </summary>
        /// <param name="securityIdentifier">The security identifier to check.</param>
        /// <returns><see langword="true"/> if the SID is restricted; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string securityIdentifier)
        {
            return _cache.Get(securityIdentifier) is null && _restrictedSids.Contains(securityIdentifier);
        }

        /// <summary>
        /// Gets or adds a <see cref="SidString"/> for the specified security identifier.
        /// </summary>
        /// <param name="securityIdentifier">The security identifier to resolve.</param>
        /// <returns>A <see cref="SidString"/> representing the resolved security identifier.</returns>
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

        /// <summary>
        /// Adds the <see cref="SidResolutionService"/> services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
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
