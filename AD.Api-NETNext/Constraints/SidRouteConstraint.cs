using AD.Api.Core.Security;
using AD.Api.Statics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AD.Api.Constraints
{
    public sealed class SidRouteConstraint : IRouteConstraint
    {
        private const int MIN_NUMBER_OF_HYPHENS_AFTER_PREFIX = 2;
        public const string ConstraintName = "objectsid";

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            ISidResolutionService? sidSvc = httpContext?.RequestServices.GetService<ISidResolutionService>();

            return IsRouteKeyStringValue(routeKey, values, out string? routeValue)
                   &&
                   IsProperlyFormatted(routeValue)
                   &&
                   !IsSidExcluded(routeValue, sidSvc);
        }

        private static bool HasMinimumNumberOfHyphens(ReadOnlySpan<char> value)
        {
            int count = value.Count(CharConstants.HYPHEN);
            bool result = MIN_NUMBER_OF_HYPHENS_AFTER_PREFIX <= count;
            Debug.Assert(result, "The SID has less than the minimum number of required hyphens.");
            return result;
        }
        private static bool IsRouteKeyStringValue(string routeKey, RouteValueDictionary routeValues, [NotNullWhen(true)] out string? value)
        {
            value = routeValues.TryGetValue(routeKey, out object? valueObj)
                ? valueObj as string
                : null;

            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool IsProperlyFormatted(ReadOnlySpan<char> value)
        {
            if (SidString.MaxSidStringLength < value.Length)
            {
                return false;
            }

            Span<char> prefix = ['S', '-'];
            if (value.Length <= prefix.Length || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return HasMinimumNumberOfHyphens(value.Slice(prefix.Length));
        }
        private static bool IsSidExcluded(string routeValue, ISidResolutionService? sidResolutionService)
        {
            if (sidResolutionService is null)
            {
                return false;
            }

            if (sidResolutionService.RestrictedSIDs.Contains(routeValue))
            {
                return true;
            }

            foreach (string startsWith in sidResolutionService.StartsWithSIDRestrictions)
            {
                if (routeValue.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
