namespace AD.Api.Serialization.Json;

/// <summary>
/// Provides extension methods for configuring and creating JSON writers using <see cref="JsonSerializerOptions"/>.
/// </summary>
public static class JsonSerializerOptionsExtensions
{
	/// <summary>
	/// Creates a new <see cref="Utf8JsonWriter"/> instance configured according to the specified <see
	/// cref="JsonSerializerOptions"/> and writes to the provided buffer.
	/// </summary>
	/// <param name="options">The <see cref="JsonSerializerOptions"/> that specifies serialization settings such as encoding, indentation, and
	/// maximum depth for the writer.</param>
	/// <param name="bufferWriter">The buffer to which the JSON data will be written. Must not be null.</param>
	/// <returns>A <see cref="Utf8JsonWriter"/> instance that writes JSON data to the specified buffer using the provided options.</returns>
	public static Utf8JsonWriter CreateWriter(this JsonSerializerOptions options, IBufferWriter<byte> bufferWriter)
	{
		return new(bufferWriter, new JsonWriterOptions
		{
			Encoder = options.Encoder,
			IndentCharacter = options.IndentCharacter,
			Indented = options.WriteIndented,
			IndentSize = options.IndentSize,
			MaxDepth = options.MaxDepth,
			NewLine = options.NewLine,
		});
	}
}