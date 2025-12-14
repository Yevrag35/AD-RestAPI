namespace AD.Api.Serialization.Json;

public interface IJsonDeserializableOnce : IJsonOnDeserialized
{
	bool IsDeserialized { get; }
}