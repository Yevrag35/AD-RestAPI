using AD.Api.Core.Settings;
using System.Collections.Frozen;
using System.Text;
using FrozenDict = System.Collections.Frozen.FrozenDictionary<string, AD.Api.Core.Serialization.SerializerAction>;
using PropDict = System.Collections.Generic.Dictionary<string, AD.Api.Core.Serialization.SerializerAction>;

namespace AD.Api.Core.Serialization;

public interface IConversionDictionary
{
	void Add(string attributeName, SerializerAction action);
	void Add<T>(IConfigurationSection section, SerializerAction action);
	void AddMany(ReadOnlySpan<string> attributeNames, SerializerAction action);
	void AddMany(ReadOnlySpan<byte> attributeName, SerializerAction action, char separator = ' ');
}

public delegate void SerializerAction(Utf8JsonWriter writer, ref readonly SerializationContext context);

internal sealed class ConversionDictionary : IConversionDictionary
{
	private readonly PropDict _dictionary;
	private readonly Encoding _encoding;

	internal ConversionDictionary()
	{
		_dictionary = new(10, StringComparer.OrdinalIgnoreCase);
		_encoding = Encoding.UTF8;
	}

	public void Add(string key, SerializerAction action)
	{
		_dictionary.Add(key, action);
	}
	public void Add<T>(IConfigurationSection section, SerializerAction action)
	{
		foreach (string name in AttributeSet.Create<T>(section, action))
		{
			_dictionary.Add(name, action);
		}
	}
	public void AddMany(ReadOnlySpan<string> keys, SerializerAction action)
	{
		_dictionary.EnsureCapacity(keys.Length + _dictionary.Count);
		foreach (string key in keys)
		{
			this.Add(key, action);
		}
	}
	public void AddMany(ReadOnlySpan<byte> attributeNames, SerializerAction action, char separator = ' ')
	{
		int charCount = _encoding.GetCharCount(attributeNames);
		bool isRented = false;
		char[]? array = null;

		Span<char> chars = charCount <= 256
			? stackalloc char[charCount]
			: RentArray(charCount, ref array, ref isRented);

		int written = _encoding.GetChars(attributeNames, chars);
		foreach (Range section in chars.Slice(0, written).Split(separator))
		{
			_dictionary.Add(chars[section].Trim().ToString(), action);
		}

		if (isRented)
		{
			ArrayPool<char>.Shared.Return(array!);
		}
	}

	internal FrozenDict ToFrozen()
	{
		return _dictionary.ToFrozenDictionary(_dictionary.Comparer);
	}

	private static Span<char> RentArray(in int length, [NotNull] ref char[]? array, ref bool isRented)
	{
		array = ArrayPool<char>.Shared.Rent(length);
		isRented = true;
		return array.AsSpan(0, length);
	}
}

