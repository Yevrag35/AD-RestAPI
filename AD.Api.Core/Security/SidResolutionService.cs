using AD.Api.Attributes.Services;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Buffers;
using System.Collections.Frozen;
using System.ComponentModel;

namespace AD.Api.Core.Security
{
    public interface ISidResolutionService
    {
        FrozenSet<string> RestrictedSIDs { get; }
        FrozenSet<string> StartsWithSIDRestrictions { get; }
    }

    [DynamicDependencyRegistration]
    public sealed class SidResolutionService : ISidResolutionService
    {
        public FrozenSet<string> RestrictedSIDs { get; } = null!;
        public FrozenSet<string> StartsWithSIDRestrictions { get; } = null!;

        private SidResolutionService()
        {
            this.RestrictedSIDs = FrozenSet<string>.Empty;
        }
        private SidResolutionService(string[] sids)
        {
            var groups = EndsWithWildcard(sids);
            StringComparer comp = StringComparer.OrdinalIgnoreCase;

            foreach (var group in groups)
            {
                switch (group.Key)
                {
                    case true:
                        this.StartsWithSIDRestrictions ??= FrozenSet.ToFrozenSet(TrimWildcards(group), comp);
                        break;

                    case false:
                        this.RestrictedSIDs ??= FrozenSet.ToFrozenSet(group, comp);
                        break;
                }
            }

            this.RestrictedSIDs ??= FrozenSet<string>.Empty;
            this.StartsWithSIDRestrictions ??= FrozenSet<string>.Empty;
        }

        private static IEnumerable<IGrouping<bool, string>> EndsWithWildcard(IEnumerable<string> sids)
        {
            return sids.GroupBy(x => x.EndsWith('*'));
        }
        private static IEnumerable<string> TrimWildcards(IEnumerable<string> sids)
        {
            foreach (string str in sids)
            {
                ReadOnlySpan<char> chars = str.AsSpan();
                yield return chars.Slice(0, chars.IndexOf('*')).ToString();
            }
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration
                .GetRequiredSection("Settings")
                .GetRequiredSection("Restrictions")
                .GetSection("RestrictedSIDs");

            string[]? set = section.Get<string[]>(x => x.ErrorOnUnknownConfiguration = false);
            SidResolutionService sidSvc = set is not null && set.Length > 0
                ? new(set)
                : new();

            _ = services.AddSingleton<ISidResolutionService>(sidSvc);
        }
    }
}

