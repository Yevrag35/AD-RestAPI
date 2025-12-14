using AD.Api.Components;
using AD.Api.Core.Operations;
using System.Text.Json;

namespace AD.Api.Core.Serialization.Json.Ldap;

public sealed class ReplaceOperationConverter : EditOperationConverter<ReplaceDictionary, DirectoryAttributeModification[]>
{
	protected override ReplaceDictionary CreateCollection()
	{
		return [];
	}

	protected override void Deserialize(ref Utf8JsonReader reader, ReplaceDictionary collection, JsonSerializerOptions options)
	{
		ObjEither<string, string[]> first = default;
		ObjEither<string, string[]> second = default;
		while (reader.TokenType == JsonTokenType.PropertyName)
		{
			string key = reader.GetString() ?? throw new JsonException("Property keys cannot be null or empty");
			if (!reader.Read())
			{
				throw new JsonException($"Unexpected end of JSON object after property: {key}");
			}
			else if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException($"Expected start of object in property: {key}");
			}

			var replaceValues = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options) ?? [];
			if (replaceValues.Count == 0)
			{
				throw new JsonException("Expected at least one old/new value in replace operation.");
			}
			else if (replaceValues.Count == 1)
			{
				first = replaceValues.Keys.First();
				second = replaceValues.Values.FirstOrDefault()?.ToString() ?? throw new JsonException("Expected non-null value in replace operation.");
			}
			else
			{
				first = replaceValues.Keys.ToArray();
				second = replaceValues.Values.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray()!;
			}

			collection.Add(key, first, second);
			reader.Read();
		}
	}
	protected override bool IsProperStartToken(JsonTokenType type)
	{
		return JsonTokenType.StartObject == type;
	}
	public override void Write(Utf8JsonWriter writer, ReplaceDictionary value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

