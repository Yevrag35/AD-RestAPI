namespace AD.Api.Core.Settings;

/// <summary>
/// Defines a contract for types that expose environment-backed configuration members and provide access to their getter
/// and setter delegates in an unsafe manner.
/// </summary>
/// <remarks>This interface is intended for advanced scenarios where direct access to environment variable
/// mappings and member access delegates is required. It is typically used by infrastructure or tooling that needs to
/// enumerate and manipulate environment-backed properties without type safety guarantees. Use with caution, as improper
/// usage may lead to runtime errors or security risks.</remarks>
/// <typeparam name="TSelf">The type that implements the environment setting interface. Must be a reference type.</typeparam>
public interface IEnvironmentSetting<TSelf> where TSelf : class, IEnvironmentSetting<TSelf>
{
	/// <summary>
	/// Provides mappings of property/field names to getters and setters used to populate the members of <typeparamref name="TSelf"/> instances from the application's
	/// environment variables.
	/// </summary>
	/// <remarks>
	/// This is invoked by infrastructure that enumerates environment functions for types
	/// implementing <see cref="IEnvironmentSetting{TSelf}"/>. Each returned <see cref="GetSetString{TClass}"/> contains
	/// a getter delegate and a setter delegate for a single configurable property.
	/// </remarks>
	/// <returns>An array of <see cref="GetSetString{TClass}"/> describing environment-backed properties.</returns>
	static abstract GetSetString<TSelf>[] GetAccessors();
}
