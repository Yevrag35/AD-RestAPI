using AD.Api.Buffers;
using AD.Api.Components;

namespace AD.Api.Serialization.Json;

public static class JsonWriterExtensions
{
	private const int MAX_STACKALLOC = 128;
	internal const string Property_Count = "Count";
	internal const string Property_Results = "Data";

	public static void WriteLdapBoolValue(this Utf8JsonWriter writer, ReadOnlySpan<char> boolChars)
	{
		if (LdapBoolean.TryParseBool(boolChars, out bool result))
		{
			writer.WriteBooleanValue(result);
		}
		else
		{
			writer.WriteNullValue();
		}
	}

	/// <summary>
	/// Writes a boolean property value to the JSON output using the specified naming policy.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy to format the property name.</param>
	/// <param name="value">The boolean value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, which is automatically formatted to remove any path prefixes.
	/// This parameter is captured using the caller argument expression.
	/// </param>
	public static void WriteBoolean(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		bool value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteBooleanValue(value);
	}

	/// <summary>
	/// Writes a boolean property value to the JSON output using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The boolean value to write.</param>
	public static void WriteBoolean(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		bool value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteBooleanValue(value);
	}

	/// <summary>
	/// Writes a nullable boolean property value to the JSON output using a UTF-8 encoded property name.
	/// If the value is <see langword="null"/>, a JSON null value is written.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The nullable boolean value to write.</param>
	public static void WriteBoolean(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		bool? value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);

		if (value.HasValue)
			writer.WriteBooleanValue(value.Value);
		else
			writer.WriteNullValue();
	}

	/// <summary>
	/// Writes an empty JSON array to the output.
	/// </summary>
	/// <param name="writer">The JSON writer to which the empty array is written.</param>
	/// <remarks>
	/// This method is equivalent to calling <see cref="Utf8JsonWriter.WriteStartArray"/> followed immediately by 
	/// <see cref="Utf8JsonWriter.WriteEndArray"/>.
	/// </remarks>
	public static void WriteEmptyArray(this Utf8JsonWriter writer)
	{
		writer.WriteStartArray();
		writer.WriteEndArray();
	}

	/// <summary>
	/// Writes an empty JSON array for the "Results" property.
	/// </summary>
	/// <param name="writer">The JSON writer to which the empty array is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	public static void WriteEmptyResultsArray(this Utf8JsonWriter writer, WorkingNamingPolicy policy)
	{
		WriteResultsBeginning(writer, 0, policy);
		writer.WriteEmptyArray();
		WriteResultsEnding(writer);
	}

	/// <summary>
	/// Writes a JSON array for the "Results" property, including the count and serialized values.
	/// </summary>
	/// <typeparam name="T">The type of the values in the array.</typeparam>
	/// <param name="writer">The JSON writer to which the array is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="count">The number of values in the array.</param>
	/// <param name="values">The values to serialize into the array.</param>
	public static void WriteResultsArray<T>(this Utf8JsonWriter writer, WorkingNamingPolicy policy, int count, IEnumerable<T?> values)
	{
		if (count == 0)
		{
			writer.WriteEmptyResultsArray(policy);
			return;
		}

		WriteResultsBeginning(writer, count, policy);
		writer.WriteStartArray();
		int index = 0;
		foreach (T? value in values)
		{
			if (value is not null)
			{
				JsonSerializer.Serialize(writer, value, policy.Options);
				index++;
			}

			if (index == count)
				break;
		}

		writer.WriteEndArray();
		WriteResultsEnding(writer);
	}

	/// <summary>
	/// Writes a JSON array for the "Results" property, including the count and serialized values.
	/// </summary>
	/// <typeparam name="T">The type of the values in the array.</typeparam>
	/// <param name="writer">The JSON writer to which the array is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="values">The values to serialize into the array.</param>
	public static void WriteResultsArray<T>(this Utf8JsonWriter writer, WorkingNamingPolicy policy, params ReadOnlySpan<T> values)
	{
		if (values.IsEmpty)
		{
			writer.WriteEmptyResultsArray(policy);
			return;
		}

		WriteResultsBeginning(writer, values.Length, policy);
		writer.WriteStartArray();
		foreach (T? value in values)
		{
			if (value is not null)
			{
				JsonSerializer.Serialize(writer, value, policy.Options);
			}
		}

		writer.WriteEndArray();
		WriteResultsEnding(writer);
	}

	/// <summary>
	/// Writes the beginning of the "Results" object, including the count property.
	/// </summary>
	/// <param name="writer">The JSON writer to which the object is written.</param>
	/// <param name="count">The count value to write.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	private static void WriteResultsBeginning(Utf8JsonWriter writer, int count, WorkingNamingPolicy policy)
	{
		writer.WriteStartObject();
		writer.WriteNumber(policy, count, Property_Count);
		policy.WritePropertyName(writer, Property_Results);
	}

	/// <summary>
	/// Writes the ending of the "Results" object.
	/// </summary>
	/// <param name="writer">The JSON writer to which the object is written.</param>
	private static void WriteResultsEnding(Utf8JsonWriter writer)
	{
		writer.WriteEndObject();
	}

	/// <summary>
	/// Writes an empty JSON object to the output.
	/// </summary>
	/// <param name="writer">The JSON writer to which the empty object is written.</param>
	/// <remarks>
	/// This method is equivalent to calling <see cref="Utf8JsonWriter.WriteStartObject"/> followed immediately by 
	/// <see cref="Utf8JsonWriter.WriteEndObject"/>.
	/// </remarks>
	public static void WriteEmptyObject(this Utf8JsonWriter writer)
	{
		writer.WriteStartObject();
		writer.WriteEndObject();
	}

	/// <summary>
	/// Writes a property whose value implements <see cref="ISpanFormattable"/> to the JSON output.
	/// The value is formatted into a character buffer before being written as a string.
	/// </summary>
	/// <typeparam name="T">The type of the formattable value.</typeparam>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The value to format and write.</param>
	/// <param name="maxLength">
	/// The maximum length for the formatted output. Determines the buffer size used for formatting.
	/// </param>
	/// <param name="propertyName">
	/// The name of the property, which is automatically formatted to remove any path prefixes.
	/// This parameter is captured using the caller argument expression.
	/// </param>
	/// <param name="format">An optional format string to apply during formatting.</param>
	/// <param name="provider">An optional format provider to use during formatting.</param>
	/// <exception cref="FormatException">Thrown if the value cannot be formatted into the provided buffer.</exception>
	public static void WriteFormattable<T>(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		T value,
		int maxLength,
		[CallerArgumentExpression(nameof(value))] string propertyName = "",
		ReadOnlySpan<char> format = default,
		IFormatProvider? provider = null)
			where T : ISpanFormattable
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);

		using (var buffer = RentedBuffer.Rent<char>(
			maxLength <= MAX_STACKALLOC
				? stackalloc char[maxLength]
				: maxLength))
		{
			if (!value.TryFormat(buffer.Span, out int charsWritten, format, provider))
			{
				throw new FormatException("Failed to format value.");
			}

			writer.WriteStringValue(buffer[..charsWritten]);
		}
	}

	/// <summary>
	/// Writes an integer property value to the JSON output using the specified naming policy.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The integer value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteNumber(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		int value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteNumberValue(value);
	}

	/// <summary>
	/// Writes a nullable integer property value to the JSON output using the specified naming policy.
	/// If the value is <see langword="null"/>, a JSON null is written.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The nullable integer value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteNumber(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		int? value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		if (value.HasValue)
		{
			writer.WriteNumber(policy, value.Value, propertyName);
		}
		else
		{
			WriteNullValue(writer, policy, propertyName);
		}
	}

	/// <summary>
	/// Writes an integer property value to the JSON output using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The integer value to write.</param>
	public static void WriteNumber(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		int value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteNumberValue(value);
	}

	/// <summary>
	/// Writes a long integer property value to the JSON output using the specified naming policy.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The long integer value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteNumber(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		long value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteNumberValue(value);
	}

	/// <summary>
	/// Writes a double precision floating-point property value to the JSON output using the specified naming policy.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The double value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteNumber(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		double value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteNumberValue(value);
	}

	/// <summary>
	/// Writes an object property to the JSON output using a specified type for serialization.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="valueType">The type of the value, used for serialization.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteObject(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		object? value,
		Type valueType,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		JsonSerializer.Serialize(writer, value, valueType, policy.Options);
	}

	/// <summary>
	/// Writes an object property to the JSON output using the runtime type of the value.
	/// If the value is <see langword="null"/>, a JSON null is written.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="propertyName">
	/// The name of the property, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteObjectOfBaseType(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		object? value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);

		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value, value.GetType(), policy.Options);
	}

	/// <summary>
	/// Writes an object property to the JSON output using the runtime type of the value and a UTF-8 encoded property name.
	/// If the value is <see langword="null"/>, a JSON null is written.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The object value to write.</param>
	public static void WriteObjectOfBaseType(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		object? value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);

		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value, value.GetType(), policy.Options);
	}

	/// <summary>
	/// Writes an object property to the JSON output using the runtime type of the value and a property name provided as a string.
	/// If the value is <see langword="null"/>, a JSON null is written.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="propertyName">The property name provided as a string.</param>
	public static void WriteObjectOfBaseType(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		object? value,
		ReadOnlySpan<char> propertyName)
	{
		policy.WritePropertyName(writer, propertyName);

		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		JsonSerializer.Serialize(writer, value, value.GetType(), policy.Options);
	}

	/// <summary>
	/// Writes an object of type <typeparamref name="T"/> to the JSON output using a UTF-8 encoded property name.
	/// If the value is <see langword="null"/>, a JSON null is written.
	/// </summary>
	/// <typeparam name="T">The type of the object to write.</typeparam>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The object value to write.</param>
	public static void WriteObject<T>(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		T? value)
	{
		if (value is null)
		{
			writer.WriteNull(utf8PropertyName);
			return;
		}

		policy.WritePropertyName(writer, utf8PropertyName);
		JsonSerializer.Serialize(writer, value, policy.Options);
	}

	/// <summary>
	/// Writes a property name and starts a JSON array.
	/// </summary>
	/// <param name="writer">The JSON writer to which the array is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="propertyName">The name of the property, provided as a read-only span of characters.</param>
	public static void WriteStartArray(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<char> propertyName)
	{
		policy.WritePropertyName(writer, propertyName);
		writer.WriteStartArray();
	}

	/// <summary>
	/// Writes a UTF-8 encoded property name and starts a JSON array.
	/// </summary>
	/// <param name="writer">The JSON writer to which the array is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	public static void WriteStartArray(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStartArray();
	}

	/// <summary>
	/// Writes a property name and starts a JSON object.
	/// </summary>
	/// <param name="writer">The JSON writer to which the object is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="propertyName">The name of the property, provided as a read-only span of characters.</param>
	public static void WriteStartObject(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<char> propertyName)
	{
		policy.WritePropertyName(writer, propertyName);
		writer.WriteStartObject();
	}

	/// <summary>
	/// Writes a UTF-8 encoded property name and starts a JSON object.
	/// </summary>
	/// <param name="writer">The JSON writer to which the object is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	public static void WriteStartObject(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStartObject();
	}

	/// <summary>
	/// Writes a DateTime value as a string property to the JSON output using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="dateTime">The DateTime value to write.</param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		DateTime dateTime)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStringValue(dateTime);
	}

	/// <summary>
	/// Writes a string property to the JSON output using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the string is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The string value to write.</param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		string? value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStringValue(value);
	}

	/// <summary>
	/// Writes a string property, provided as a read-only span of characters, to the JSON output 
	/// using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the string is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="value">The string value as a read-only span of characters to write.</param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		ReadOnlySpan<char> value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStringValue(value);
	}
	/// <summary>
	/// Writes a string property, provided as a read-only span of characters, to the JSON output 
	/// using a UTF-8 encoded property name.
	/// </summary>
	/// <param name="writer">The JSON writer to which the string is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="utf8PropertyName">The UTF-8 encoded property name.</param>
	/// <param name="utf8Value">The string value as a read-only span of characters to write.</param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<byte> utf8PropertyName,
		ReadOnlySpan<byte> utf8Value)
	{
		policy.WritePropertyName(writer, utf8PropertyName);
		writer.WriteStringValue(utf8Value);
	}

	/// <summary>
	/// Writes a string property to the JSON output using a property name provided as a string.
	/// The property name is formatted to remove any prefixed paths before being used.
	/// </summary>
	/// <param name="writer">The JSON writer to which the string is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The string value to write.</param>
	/// <param name="propertyName">
	/// The property name, which is processed to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		ReadOnlySpan<char> value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteStringValue(value);
	}

	/// <summary>
	/// Writes a string property to the JSON output using a property name provided as a string.
	/// The property name is formatted to remove any prefixed paths before being used.
	/// </summary>
	/// <param name="writer">The JSON writer to which the string is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The string value to write.</param>
	/// <param name="propertyName">
	/// The property name, which is processed to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	public static void WriteString(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		string? value,
		[CallerArgumentExpression(nameof(value))] string propertyName = "")
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteStringValue(value);
	}

	/// <summary>
	/// Writes a property whose value implements <see cref="IUtf8SpanFormattable"/> to the JSON output.
	/// The value is formatted into a rented byte buffer and written as a string.
	/// </summary>
	/// <typeparam name="T">The type of the formattable value.</typeparam>
	/// <param name="writer">The JSON writer to which the value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="value">The value to format and write.</param>
	/// <param name="maxLength">
	/// The maximum length (in characters) for the formatted value. This value is converted to the maximum byte count 
	/// for the buffer allocation.
	/// </param>
	/// <param name="propertyName">
	/// The property name, automatically formatted to remove any path prefixes.
	/// Captured via the caller argument expression.
	/// </param>
	/// <param name="format">An optional format string to apply during formatting.</param>
	/// <param name="provider">An optional format provider to use during formatting.</param>
	/// <exception cref="FormatException">Thrown if the value cannot be formatted into the provided buffer.</exception>
	public static void WriteUtf8Formattable<T>(this Utf8JsonWriter writer,
		WorkingNamingPolicy policy,
		T value,
		int maxLength,
		[CallerArgumentExpression(nameof(value))] string propertyName = "",
		ReadOnlySpan<char> format = default,
		IFormatProvider? provider = null)
		where T : IUtf8SpanFormattable
	{
		maxLength = Encoding.UTF8.GetMaxByteCount(maxLength);

		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);

		using (var buffer = RentedBuffer.Rent<byte>(
			maxLength <= MAX_STACKALLOC
				? stackalloc byte[maxLength]
				: maxLength))
		{
			if (!value.TryFormat(buffer.Span, out int bytesWritten, format, provider))
			{
				throw new FormatException("Failed to format value.");
			}

			writer.WriteStringValue(buffer.Slice(0, bytesWritten));
		}
	}

	/// <summary>
	/// Formats the property name by returning the substring after the last occurrence of a period.
	/// If no period is found, the original property name is returned.
	/// </summary>
	/// <param name="propertyName">The property name to format.</param>
	/// <returns>
	/// A read-only span of characters representing the formatted property name.
	/// </returns>
	private static ReadOnlySpan<char> FormatPropertyName(ReadOnlySpan<char> propertyName)
	{
		if (TryGetLastIndexOfPeriod(propertyName, out int index))
		{
			propertyName = propertyName.Slice(index + 1);
		}

		return propertyName;
	}

	/// <summary>
	/// Attempts to find the last index of a period in the property name.
	/// </summary>
	/// <param name="propertyName">The property name to search.</param>
	/// <param name="index">
	/// When this method returns, contains the index position of the period if found and valid; otherwise, an undefined value.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if a period is found and it is not the last character; otherwise, <see langword="false"/>.
	/// </returns>
	private static bool TryGetLastIndexOfPeriod(ReadOnlySpan<char> propertyName, out int index)
	{
		return propertyName.TryLastIndexOf('.', out index)
			&& index < propertyName.Length - 1;
	}

	/// <summary>
	/// Writes a JSON null value for a property using the specified naming policy.
	/// </summary>
	/// <param name="writer">The JSON writer to which the null value is written.</param>
	/// <param name="policy">A reference to the working naming policy used to format the property name.</param>
	/// <param name="propertyName">The name of the property for which a null value is written.</param>
	private static void WriteNullValue(Utf8JsonWriter writer, WorkingNamingPolicy policy, ReadOnlySpan<char> propertyName)
	{
		ReadOnlySpan<char> propName = FormatPropertyName(propertyName);
		policy.WritePropertyName(writer, propName);
		writer.WriteNullValue();
	}
}
