using AD.Api.Statics;
using AD.Api.Unmanaged;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace AD.Api.Core.Ldap.Filters;

[StructLayout(LayoutKind.Auto)]
public ref struct FilterSpanWriter
{
	private SpanStringBuilder _builder;
	private int _depth;

	public readonly int Depth => _depth;
	public readonly int Length => _builder.Length;

	public FilterSpanWriter(int initialCapacity)
		: this(new SpanStringBuilder(initialCapacity))
	{
	}
	public FilterSpanWriter(Span<char> buffer)
		: this(new SpanStringBuilder(buffer))
	{
	}
	public FilterSpanWriter(SpanStringBuilder builder)
	{
		_builder = builder;
		_depth = 0;
	}

	public FilterSpanWriter And()
	{
		this.WriteKeyword(['(', '&']);
		return this;
	}

	/// <inheritdoc cref="ValidateEnd" path="/exception"/>
	public void End()
	{
		this.ValidateEnd();

		_builder.Append(')');
		_depth--;
	}
	public void EndAll()
	{
		if (_depth > 0)
		{
			int howMany = _depth;
			_builder.Append(')', howMany);
			_depth = 0;
		}
	}

	public void Equal(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> value)
	{
		_builder.Append('(');
		_builder.Append(propertyName.Trim());
		_builder.Append('=');
		_builder.Append(value.Trim());
		_builder.Append(')');
	}
	public void Equal(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> modifier, scoped ReadOnlySpan<char> value)
	{
		if (modifier.IsWhiteSpace())
		{
			this.Equal(propertyName, value);
			return;
		}

		Span<char> combined = stackalloc char[propertyName.Length + modifier.Length];
		propertyName.CopyTo(combined);
		modifier.CopyTo(combined.Slice(propertyName.Length));

		this.Equal(combined, value);
	}
	public void Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> value)
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8PropertyName.Length);
		Span<char> nameChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

		this.Equal(nameChars.Slice(0, count), value);
	}
	public void Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> modifier, scoped ReadOnlySpan<char> value)
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8PropertyName.Length);
		Span<char> nameChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

		this.Equal(nameChars.Slice(0, count), modifier, value);
	}
	public void Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<byte> utf8Value)
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8Value.Length);
		Span<char> valChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, valChars);

		this.Equal(utf8PropertyName, valChars.Slice(0, count));
	}

	public void Equal<T>(scoped ReadOnlySpan<char> propertyName, T value)
		where T : unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryInteger<T>, ISpanFormattable
	{
		int length = value.GetLength();
		Span<char> intChars = stackalloc char[LengthConstants.INT128_MAX];
		_ = value.TryFormat(intChars, out int intLength, default, CultureInfo.InvariantCulture);

		this.Equal(propertyName, intChars.Slice(0, intLength));
	}
	public void Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, T value)
		where T : unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryInteger<T>, ISpanFormattable
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8PropertyName.Length);
		Span<char> nameChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

		this.Equal(nameChars.Slice(0, count), value);
	}
	public void Equal<T>(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> modifier, T value)
		where T : unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryInteger<T>, ISpanFormattable
	{
		if (modifier.IsWhiteSpace())
		{
			this.Equal(propertyName, value);
			return;
		}

		Span<char> combined = stackalloc char[propertyName.Length + modifier.Length];
		propertyName.CopyTo(combined);
		modifier.CopyTo(combined.Slice(propertyName.Length));

		this.Equal(combined, value);
	}
	public void Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> modifier, T value)
		where T : unmanaged, INumber<T>, IMinMaxValue<T>, IBinaryInteger<T>, ISpanFormattable
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8PropertyName.Length);
		Span<char> nameChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

		this.Equal(nameChars.Slice(0, count), modifier, value);
	}

	public void Equal<T>(scoped ReadOnlySpan<char> propertyName, T value, int maxValueLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
		where T : ISpanFormattable
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxValueLength);
		Span<char> valChars = stackalloc char[maxValueLength];
		if (!value.TryFormat(valChars, out int written, format, provider))
		{
			throw new FormatException("Unable to format the value to the character span.");
		}

		this.Equal(propertyName, valChars.Slice(0, written));
	}
	public void Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, T value, int maxValueLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
		where T : ISpanFormattable
	{
		int count = Encoding.UTF8.GetMaxCharCount(utf8PropertyName.Length);
		Span<char> nameChars = stackalloc char[count];
		count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

		this.Equal(nameChars.Slice(0, count), value, maxValueLength, format, provider);
	}
	public void Equal<T>(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> modifier, T value, int maxValueLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
	{
		Span<char> valChars = stackalloc char[maxValueLength];
		if (!value.TryFormat(valChars, out int written, format, provider))
		{
			throw new FormatException("Unable to format the value to the character span.");
		}

		this.Equal(propertyName, modifier, valChars.Slice(0, written));
	}

	public void Not()
	{
		this.WriteKeyword(['(', '!']);
	}
	public void NotEqual(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> value)
	{
		_builder.AppendChars('(', '!', '(');
		_builder.Append(propertyName.Trim());
		_builder.Append('=');
		_builder.Append(value.Trim());
		_builder.Append(')', 2);
	}

	public void Or()
	{
		this.WriteKeyword(['(', '|']);
	}
	public void Start()
	{
		_builder.Append('(');
		_depth++;
	}

	internal void WriteRaw(scoped ReadOnlySpan<char> rawText)
	{
		_builder.Append(rawText);
	}

	public readonly ReadOnlySpan<char> AsSpan()
	{
		return _builder.AsSpan();
	}
	/// <inheritdoc cref="SpanStringBuilder.Build"/>
	public string Build()
	{
		string str = _builder.ToString();
		this.Dispose();
		return str;
	}
	public void Dispose()
	{
		SpanStringBuilder builder = _builder;
		this = default;
		builder.Dispose();
	}
	/// <inheritdoc cref="SpanStringBuilder.ToString"/>
	public override readonly string ToString()
	{
		return _builder.ToString();
	}
	/// <exception cref="InvalidOperationException"></exception>
	private readonly void ValidateEnd()
	{
		if (_depth <= 0)
		{
			throw new InvalidOperationException("The filter depth is already at zero.");
		}
	}
	private void WriteKeyword(scoped Span<char> keywordChars)
	{
		_builder.Append(keywordChars);
		_depth++;
	}
}