using AD.Api.Core.Operations;
using System.Text.Json;

namespace AD.Api.Core.Serialization.Json.Ldap;

public sealed class ClearOperationConverter : EditOperationConverter<ClearDictionary, DirectoryAttributeModification>
{
	protected override ClearDictionary CreateCollection()
	{
		return [];
	}
	protected override void Deserialize(ref Utf8JsonReader reader, ClearDictionary collection, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.String)
		{
			string propertyName = reader.GetString() ?? throw new JsonException("Property names cannot be null or empty.");

			collection.Add(propertyName);
			reader.Read();
			return;
		}

		string[] names = JsonSerializer.Deserialize<string[]>(ref reader, options) ?? [];
		foreach (string name in names)
		{
			collection.Add(name);
		}
	}
	protected override bool IsProperStartToken(JsonTokenType type)
	{
		return type switch
		{
			JsonTokenType.StartArray or JsonTokenType.String => true,
			_ => false,
		};
	}
	public override void Write(Utf8JsonWriter writer, ClearDictionary value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

