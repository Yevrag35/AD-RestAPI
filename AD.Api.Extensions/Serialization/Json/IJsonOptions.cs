namespace AD.Api.Serialization.Json;

/// <summary>
/// Provides configuration options for JSON serialization and deserialization operations.
/// </summary>
/// <remarks>Implementations of this interface supply a configured instance of <see
/// cref="JsonSerializerOptions"/> that controls serialization behavior, such as property naming
/// policies, converters, and formatting. Use this interface to standardize JSON settings across the Krubera applications.</remarks>
public interface IJsonOptions
{
	/// <summary>
	/// Gets the options used to configure JSON serialization and deserialization behavior.
	/// </summary>
	JsonSerializerOptions SerializerOptions { get; }
}