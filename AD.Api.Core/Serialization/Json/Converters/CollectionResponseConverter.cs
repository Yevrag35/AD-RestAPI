using AD.Api.Core.Web;
using AD.Api.Serialization.Converters;
using AD.Api.Serialization.Json;

namespace AD.Api.Core.Serialization.Json.Converters;

public sealed class CollectionResponseConverter : ByPolicyJsonConverter<CollectionResponse>
{
	public CollectionResponseConverter(WorkingNamingPolicy namingPolicy) : base(namingPolicy)
	{
	}

	public override CollectionResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotSupportedException();
	}

	public override void Write(Utf8JsonWriter writer, CollectionResponse value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value.Result is not null)
		{
			this.NamingPolicy.WritePropertyName(writer, "Result"u8);
			JsonSerializer.Serialize(writer, value.Result, value.Result.GetType(), options);

			if (value.ErrorCode.HasValue)
			{
				this.NamingPolicy.WritePropertyName(writer, "ErrorCode"u8);
				writer.WriteNumberValue(value.ErrorCode.Value);
			}

			if (value.AddResultCode)
			{
				Enum result = value.Result;
				this.NamingPolicy.WritePropertyName(writer, "ResultCode"u8);
				writer.WriteNumberValue(Convert.ToInt32(result));
			}
		}

		if (!string.IsNullOrEmpty(value.Error))
		{
			this.NamingPolicy.WritePropertyName(writer, "errorMessage"u8);
			writer.WriteStringValue(value.Error);
		}

		this.NamingPolicy.WritePropertyName(writer, "Count"u8);
		writer.WriteNumberValue(value.Count);

		//if (!string.IsNullOrEmpty(value.NextPageUrl))
		//{

		//   this.NamingPolicy.WritePropertyName(writer, "NextPageUrl"u8);
		//   writer.WriteStringValue(value.NextPageUrl);
		//}

		this.NamingPolicy.WritePropertyName(writer, "Data"u8);
		writer.WriteStartArray();
		if (value.Count > 0)
		{
			Type? itemType = null;

			IEnumerator enumerator = value.Data.GetEnumerator();
			int count = 0;
			while (count < value.Count && enumerator.MoveNext())
			{
				object item = enumerator.Current;
				itemType ??= item.GetType();
				JsonSerializer.Serialize(writer, item, itemType, options);
				count++;
			}

			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}

		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}

