namespace AD.Api.Startup;

/// <summary>
/// A struct used at application startup to force loading of assemblies by referencing a type
/// from each assembly.
/// </summary>
public readonly ref struct Referencer
{
	/// <summary>
	/// References the specified type. No action is taken.
	/// </summary>
	/// <typeparam name="T">The type within an assembly to load.</typeparam>
	/// <returns>
	///     The same <see cref="Referencer"/> for chaining.
	/// </returns>
	public readonly Referencer Reference<T>()
	{
		return this;
	}
	/// <summary>
	/// References the specified type. No action is taken.
	/// </summary>
	/// <param name="type">The type within an assembly to load.</param>
	/// <returns>
	///     The same <see cref="Referencer"/> for chaining.
	/// </returns>
	public readonly Referencer Reference(Type type)
	{
		return this;
	}

	/// <summary>
	/// Force loads referenced assemblies by executing the specified action.
	/// </summary>
	/// <param name="action">The action to execute.</param>
	public static void LoadAll(Action<Referencer> action)
	{
		action(default);
	}
}

