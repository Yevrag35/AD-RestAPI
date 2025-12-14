using AD.Api.Buffers;
using AD.Api.Core.Security;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AD.Api.Constraints;

public sealed class SidRouteConstraint : IRouteConstraint
{
	private const int MIN_NUMBER_OF_HYPHENS_AFTER_PREFIX = 2;
	public const string ConstraintName = "objectsid";

	public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
	{
		IRestrictedSids? sidSvc = httpContext?.RequestServices.GetService<IRestrictedSids>();

		return IsRouteKeyStringValue(routeKey, values, out string? routeValue)
			   &&
			   IsProperlyFormatted(routeValue)
			   &&
			   !IsSidExcluded(routeValue, sidSvc);
	}

	private static bool HasMinimumNumberOfHyphens(ReadOnlySpan<char> value)
	{
		int count = value.Count('-');
		bool result = MIN_NUMBER_OF_HYPHENS_AFTER_PREFIX <= count;
		Debug.Assert(result, "The SID has less than the minimum number of required hyphens.");
		return result;
	}
	private static bool IsProperlyFormatted(ReadOnlySpan<char> value)
	{
		if (!SidString.IsCharLengthInRange(value))
		{
			return false;
		}

		Span<char> prefix = ['S', '-'];

		return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
			&& SectionsAreValid(value.Slice(prefix.Length))
			&& HasMinimumNumberOfHyphens(value.Slice(prefix.Length + 1));   // + 1 = the next character
																			//       after the first hyphen should
																			//       never be a hyphen so we skip
																			//       so it doesn't skew the count 
																			//       which would end up being an
																			//       invalid SID anyway.
	}
	private static bool SectionsAreValid(ReadOnlySpan<char> value)
	{
		foreach (Range section in value.Split('-'))
		{
			ReadOnlySpan<char> span = value[section].Trim();
			if (span.IsEmpty || span.ContainsAnyExcept(CharCollections.Numbers))
			{
				return false;
			}
		}

		return true;
	}
	private static bool IsRouteKeyStringValue(string routeKey, RouteValueDictionary routeValues, [NotNullWhen(true)] out string? value)
	{
		value = routeValues.TryGetValue(routeKey, out object? valueObj)
			? valueObj as string
			: null;

		return value is not null;
	}
	private static bool IsSidExcluded(string routeValue, IRestrictedSids? restrictedSids)
	{
		return restrictedSids?.Contains(routeValue) ?? false;
	}
}
