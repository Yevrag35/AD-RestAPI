namespace AD.Api.Core.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="PathString"/> to evaluate whether a path starts with any of a set of specified
/// segments.
/// </summary>
public static class PathStringExtensions
{
	/// <summary>
	/// Determines whether the specified path starts with any of the provided path segments, using a case-insensitive
	/// comparison.
	/// </summary>
	/// <param name="path">The path to evaluate for matching segments.</param>
	/// <param name="values">A set of path segments to compare against the start of the specified path.</param>
	/// <returns>true if the path starts with any of the specified segments; otherwise, false.</returns>
	public static bool StartsWithAnySegments(this PathString path, params ReadOnlySpan<PathString> values)
	{
		return StartsWithAnySegments(path, StringComparison.OrdinalIgnoreCase, values);
	}
	/// <summary>
	/// Determines whether the current path starts with any of the specified path segments, using the given string
	/// comparison option.
	/// </summary>
	/// <param name="path">The path to evaluate for matching segments.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison, such as case sensitivity.</param>
	/// <param name="values">A set of path segments to compare against the start of the current path.</param>
	/// <returns>true if the path starts with any of the specified segments; otherwise, false.</returns>
	public static bool StartsWithAnySegments(this PathString path, StringComparison comparisonType, params ReadOnlySpan<PathString> values)
	{
		foreach (PathString value in values)
		{
			if (path.StartsWithSegments(value, comparisonType))
				return true;
		}

		return false;
	}
}
