using AD.Api.Attributes;
using AD.Api.Ldap.Enums;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections;

namespace AD.Api.Enums.Internal
{
    [DebuggerDisplay(@"\{EnumCount = {EnumCount}; ValueCount = {ValueCount}\}")]
    internal sealed class EVDictionary<TEnum, TAtt, TValue> : IEnumValues<TEnum, TAtt, TValue>, IEnumerable<TEnum>
        where TEnum : unmanaged, Enum
        where TAtt : Attribute, IValuedAttribute<TValue>
        where TValue : notnull
    {
        [DebuggerStepThrough]
        private readonly struct Dicts
        {
            internal readonly IReadOnlyDictionary<string, TAtt> Attributes;
            internal readonly IEnumStrings<TEnum> EnumStrings;

            internal Dicts(IReadOnlyDictionary<string, TAtt> attributes, IEnumStrings<TEnum> enumStrings)
            {
                Attributes = attributes;
                EnumStrings = enumStrings;
            }
        }

        private readonly Dicts _backing;

        public string this[TEnum key] => _backing.EnumStrings[key];

        public int EnumCount => _backing.EnumStrings.NameCount;
        public IEnumStrings<TEnum> EnumStrings => _backing.EnumStrings;
        public int ValueCount => _backing.Attributes.Count;

        [DebuggerStepThrough]
        public EVDictionary(IEnumStrings<TEnum> enumStrings)
            : this(CreateFromEnumsAndFields(enumStrings), enumStrings)
        {
        }
        [DebuggerStepThrough]
        private EVDictionary(IDictionary<string, TAtt> nameToValue, IEnumStrings<TEnum> enumStrings)
        {
            IReadOnlyDictionary<string, TAtt> attDict = enumStrings.IsFrozen
                ? nameToValue.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase)
                : nameToValue.AsReadOnly();

            _backing = new(attDict, enumStrings);
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool ContainsEnum(TEnum key)
        {
            return _backing.EnumStrings.ContainsEnum(key);
        }

        [DebuggerStepThrough]
        public IEnumerator<TEnum> GetEnumerator()
        {
            return _backing.EnumStrings.Values.GetEnumerator();
        }
        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [DebuggerStepThrough]
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public TValue? GetValue(TEnum key, [AllowNull] TValue defaultValue = default)
        {
            return this.TryGetAttribute(key, out TAtt? attribute)
                ? attribute.Value
                : defaultValue;
        }
        [DebuggerStepThrough]
        public bool TryGetAttribute(TEnum key, [NotNullWhen(true)] out TAtt? attribute)
        {
            if (_backing.EnumStrings.TryGetName(key, out string? name)
                &&
                _backing.Attributes.TryGetValue(name, out attribute))
            {
                return true;
            }
            else
            {
                attribute = null;
                return false;
            }
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryGetValue(TEnum key, [NotNullWhen(true)] out TValue? value)
        {
            if (this.TryGetAttribute(key, out TAtt? attribute))
            {
                value = attribute.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        private static Dictionary<string, TAtt> CreateFromEnumsAndFields(IEnumStrings<TEnum> enumStrings)
        {
            FieldInfo[] fields = ArrayPool<FieldInfo>.Shared.Rent(int.Max(enumStrings.NameCount, 32));
            Span<FieldInfo> fieldSpan = fields.AsSpan(0, enumStrings.NameCount);
            Type attType = typeof(TAtt);

            int written = PopulateNamedFields(enumStrings, attType, fieldSpan);
            if (written < fieldSpan.Length)
            {
                fieldSpan = fieldSpan.Slice(0, written);
            }

            var dict = CreateDictionary(fieldSpan);
            ArrayPool<FieldInfo>.Shared.Return(fields);
            return dict;
        }
        private static Dictionary<string, TAtt> CreateDictionary(Span<FieldInfo> fields)
        {
            Dictionary<string, TAtt> nameToAtt = new(fields.Length, StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo fi in fields)
            {
                var att = fi.GetCustomAttributes<TAtt>(inherit: false).First();
                nameToAtt.TryAdd(fi.Name, att);
            }

            return nameToAtt;
        }
        private static int PopulateNamedFields(IEnumStrings<TEnum> enumStrings, Type attributeType, Span<FieldInfo> fieldsSpan)
        {
            Type enumType = typeof(TEnum);

            string[] array = ArrayPool<string>.Shared.Rent(enumStrings.NameCount);
            Span<string> span = array.AsSpan(0, enumStrings.NameCount);
            int count = enumStrings.CopyNamesTo(span);

            if (count < span.Length)
            {
                span = span.Slice(0, count);
            }

            int written = 0;
            foreach (string name in span)
            {
                FieldInfo? fi = enumType.GetField(name);
                if (fi is not null && fi.IsDefined(attributeType, inherit: false))
                {
                    fieldsSpan[written++] = fi;
                }
            }

            ArrayPool<string>.Shared.Return(array);
            return written;
        }
    }
}

