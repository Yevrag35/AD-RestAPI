using AD.Api.Attributes.Services;
using System.Collections.Frozen;
using System.ComponentModel;

namespace AD.Api.Core.Security
{
    public interface IRestrictedSids
    {
        bool Contains(string securityIdentifier);
    }

    [DynamicDependencyRegistration]
    internal abstract class RestrictedSids : IRestrictedSids
    {
        internal abstract bool IsEmpty { get; }

        public abstract bool Contains(string securityIdentifier);

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration
                .GetRequiredSection("Settings")
                .GetSection("Restrictions")
                .GetSection("RestrictedSIDs");

            string[]? set = section.Get<string[]>();

            _ = set is null || set.Length <= 0
                ? services.AddSingleton<RestrictedSids, NoRestrictedSids>()
                : services.AddSingleton<RestrictedSids>(new HasRestrictedSids(set));

            if (!OperatingSystem.IsWindows())
            {
                _ = services.AddSingleton<IRestrictedSids>(x => x.GetRequiredService<RestrictedSids>());
            }
        }

        private sealed class HasRestrictedSids : RestrictedSids
        {
            private readonly FrozenSet<string> _sids;

            internal override bool IsEmpty => false;

            internal HasRestrictedSids(string[] sids)
            {
                _sids = FrozenSet.ToFrozenSet(sids, StringComparer.OrdinalIgnoreCase);
            }

            public override bool Contains(string securityIdentifier)
            {
                return _sids.Contains(securityIdentifier);
            }
        }
        private sealed class NoRestrictedSids : RestrictedSids
        {
            internal override bool IsEmpty => true;

            public override bool Contains(string securityIdentifier)
            {
                return false;
            }
        }
    }
}

