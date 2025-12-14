namespace AD.Api.Collections.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/> to enhance collection manipulation.
/// </summary>
public static class ListExtensions
{
	/// <summary>
	/// Adds multiple values to the end of the <see cref="ICollection{T}"/> in a single operation.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	/// <param name="list">The list to which the values will be added. Must not be <see langword="null"/>.</param>
	/// <param name="values">A <see langword="params"/> array of <see cref="ReadOnlySpan{T}"/> containing the values to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
	public static void AddMany<T>(this ICollection<T> list, params ReadOnlySpan<T> values)
	{
		ArgumentNullException.ThrowIfNull(list);

		foreach (T item in values)
		{
			list.Add(item);
		}
	}
}