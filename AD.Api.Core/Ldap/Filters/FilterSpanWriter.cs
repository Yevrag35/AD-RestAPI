using AD.Api.Statics;
using AD.Api.Strings.Spans;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
//using IFilterTypeValues = AD.Api.Enums.IEnumStrings<AD.Api.Core.Ldap.Filters.FilterTokenType>;

namespace AD.Api.Core.Ldap.Filters
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct FilterSpanWriter
    {
        private SpanStringBuilder _builder;
        private int _depth;
        //private IFilterTypeValues _filterValues;
        //private FilterTokenType _tokenType;

        public readonly int Depth => _depth;
        public readonly int Length => _builder.Length;
        //public readonly FilterTokenType TokenType => _tokenType;

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
            //_tokenType = FilterTokenType.Begin;
        }

        public FilterSpanWriter And()
        {
            this.WriteKeyword(['(', '&']);
            return this;
        }

        /// <inheritdoc cref="ValidateEnd" path="/exception"/>
        public FilterSpanWriter End()
        {
            this.ValidateEnd();

            _builder = _builder.Append(')');
            _depth--;
            return this;
        }
        public FilterSpanWriter EndAll()
        {
            if (_depth <= 0)
            {
                return this;
            }

            int howMany = _depth;
            _builder = _builder.Append(')', howMany);
            _depth = 0;
            return this;
        }

        public FilterSpanWriter Equal(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> value)
        {
            _builder = _builder.Append('(')
                               .Append(propertyName.Trim())
                               .Append('=')
                               .Append(value.Trim())
                               .Append(')');

            return this;
        }
        public FilterSpanWriter Equal(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> modifier, scoped ReadOnlySpan<char> value)
        {
            if (modifier.IsWhiteSpace())
            {
                return this.Equal(propertyName, value);
            }

            Span<char> combined = stackalloc char[propertyName.Length + modifier.Length];
            propertyName.CopyTo(combined);
            modifier.CopyTo(combined.Slice(propertyName.Length));

            return this.Equal(combined, value);
        }
        public FilterSpanWriter Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> value)
        {
            int count = Encoding.UTF8.GetCharCount(utf8PropertyName);
            Span<char> nameChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

            return this.Equal(nameChars.Slice(0, count), value);
        }
        public FilterSpanWriter Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> modifier, scoped ReadOnlySpan<char> value)
        {
            int count = Encoding.UTF8.GetCharCount(utf8PropertyName);
            Span<char> nameChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

            return this.Equal(nameChars.Slice(0, count), modifier, value);
        }
        public FilterSpanWriter Equal(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<byte> utf8Value)
        {
            int count = Encoding.UTF8.GetCharCount(utf8Value);
            Span<char> valChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, valChars);

            return this.Equal(utf8PropertyName, valChars.Slice(0, count));
        }

        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<char> propertyName, T value)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, ISpanFormattable
        {
            int length = value.GetLength();
            Span<char> intChars = stackalloc char[LengthConstants.INT128_MAX];
            _ = value.TryFormat(intChars, out int intLength, default, CultureInfo.InvariantCulture);

            return this.Equal(propertyName, intChars.Slice(0, intLength));
        }
        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, T value)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, ISpanFormattable
        {
            int count = Encoding.UTF8.GetCharCount(utf8PropertyName);
            Span<char> nameChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

            return this.Equal(nameChars.Slice(0, count), value);
        }
        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> modifier, T value)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, ISpanFormattable
        {
            if (modifier.IsWhiteSpace())
            {
                return this.Equal(propertyName, value);
            }

            Span<char> combined = stackalloc char[propertyName.Length + modifier.Length];
            propertyName.CopyTo(combined);
            modifier.CopyTo(combined.Slice(propertyName.Length));

            return this.Equal(combined, value);
        }
        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, scoped ReadOnlySpan<char> modifier, T value)
            where T : unmanaged, INumber<T>, IMinMaxValue<T>, ISpanFormattable
        {
            int count = Encoding.UTF8.GetCharCount(utf8PropertyName);
            Span<char> nameChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

            return this.Equal(nameChars.Slice(0, count), modifier, value);
        }

        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<char> propertyName, T value, int maxValueLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            where T : ISpanFormattable
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxValueLength);
            Span<char> valChars = stackalloc char[maxValueLength];
            if (!value.TryFormat(valChars, out int written, format, provider))
            {
                throw new FormatException("Unable to format the value to the character span.");
            }

            return this.Equal(propertyName, valChars.Slice(0, written));
        }
        public FilterSpanWriter Equal<T>(scoped ReadOnlySpan<byte> utf8PropertyName, T value, int maxValueLength, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
            where T : ISpanFormattable
        {
            int count = Encoding.UTF8.GetCharCount(utf8PropertyName);
            Span<char> nameChars = stackalloc char[count];
            count = Encoding.UTF8.GetChars(utf8PropertyName, nameChars);

            return this.Equal(nameChars.Slice(0, count), value, maxValueLength, format, provider);
        }

        public FilterSpanWriter Not()
        {
            this.WriteKeyword(['(', '!']);
            return this;
        }
        public FilterSpanWriter NotEqual(scoped ReadOnlySpan<char> propertyName, scoped ReadOnlySpan<char> value)
        {
            _builder = _builder.Append(['(', '!', '('])
                               .Append(propertyName.Trim())
                               .Append('=')
                               .Append(value.Trim())
                               .Append(')', 2);

            return this;
        }

        public FilterSpanWriter Or()
        {
            this.WriteKeyword(['(', '|']);
            return this;
        }
        public FilterSpanWriter Start()
        {
            _builder = _builder.Append('(');
            _depth++;
            return this;
        }

        internal FilterSpanWriter WriteRaw(scoped ReadOnlySpan<char> rawText)
        {
            _builder = _builder.Append(rawText);
            return this;
        }

        public readonly Span<char> AsSpan()
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
            _builder = _builder.Append(keywordChars);
            _depth++;
        }
    }
}