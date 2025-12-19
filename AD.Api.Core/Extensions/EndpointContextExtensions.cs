namespace AD.Api.Core.Extensions;
/// <summary>
/// Provides extension methods for working with endpoint filter invocation contexts.
/// </summary>
public static class EndpointContextExtensions
{
	/// <summary>
	/// Attempts to retrieve the non-<see langword="null"/> argument at the specified index from the endpoint filter invocation context.
	/// </summary>
	/// <remarks>This method does not throw an exception if the index is out of range or if the argument is not
	/// present. Use the return value to determine whether the argument was successfully retrieved.</remarks>
	/// <param name="context">The endpoint filter invocation context containing the arguments to search.</param>
	/// <param name="index">The zero-based index of the argument to retrieve. Must be within the bounds of the argument collection.</param>
	/// <param name="argument">When this method returns, contains the argument at the specified index if found; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the argument at the specified index was found; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetArgument(this EndpointFilterInvocationContext context, int index, [NotNullWhen(true)] out object? argument)
	{
		if (context.Arguments.Count > index && context.Arguments[index] is object o)
		{
			argument = o;
			return true;
		}

		argument = null;
		return false;
	}
}
