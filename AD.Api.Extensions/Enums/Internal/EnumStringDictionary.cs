using AD.Api.Collections;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections;
using System.Collections.ObjectModel;
using AD.Api.Strings.Extensions;
using System.Runtime.CompilerServices;

namespace AD.Api.Enums.Internal
{
    [DebuggerDisplay(@"Count = {NameCount}; Properties = \{IsFrozen = {IsFrozen}; Default = {DefaultName}\}")]
    internal sealed class ESDictionary<T> : IEnumStrings<T> where T : unmanaged, Enum
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly string _joinBy = StringExtensions.CommaSpace;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly SearchValues<char> _splitByChars;
        private readonly IReadOnlyDictionary<string, T> _byNames;
        private readonly IReadOnlyDictionary<T, string> _byValues;
        private readonly IReadOnlyDictionary<int, T> _valuesAsInt;

        /// <inheritdoc />
        public string this[T key]
        {
            [DebuggerStepThrough]
            get => _byValues.TryGetValue(key, out string? name)
                        ? name
                        : this.DefaultName ?? string.Empty;
        }

        /// <inheritdoc />
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int IReadOnlyCollection<KeyValuePair<string, T>>.Count => this.NameCount;
        /// <inheritdoc/>
        public string? DefaultName { get; }
        /// <inheritdoc/>
        public int EnumCount => _byValues.Count;
        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(DefaultName))]
        public bool HasDefaultName { get; }
        /// <inheritdoc/>
        public bool HasDuplicates { get; }
        /// <inheritdoc/>
        public bool IsIntegerBased => _valuesAsInt.Count > 0;
        /// <inheritdoc/>
        public bool IsFlagsEnum { get; }
        /// <inheritdoc/>
        public bool IsFrozen { get; }
        /// <inheritdoc/>
        public int NameCount => _byNames.Count;
        /// <inheritdoc/>
        public int TotalNameLength { get; }
        /// <inheritdoc/>
        public IEnumerable<T> Values => _byNames.Values;

        [DebuggerStepThrough]
        public ESDictionary()
            : this(CreateDictionaries(out int totalLength), totalLength, isFrozen: false)
        {
        }
        [DebuggerStepThrough]
        internal ESDictionary(bool isFrozen)
            : this(CreateDictionaries(out int totalLength), totalLength, isFrozen)
        {
        }
        private ESDictionary(DictTuple dicts, int totalLength, bool isFrozen)
        {
            this.TotalNameLength = totalLength;
            (_byValues, _byNames, _valuesAsInt) = isFrozen
                ? dicts.ToFrozen()
                : dicts.ToReadOnly();

            this.HasDefaultName = TryGetDefaultName(in dicts, out string? defaultName);
            this.IsFlagsEnum = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);
            this.DefaultName = defaultName;
            this.HasDuplicates = dicts.HasDuplicates;
            this.IsFrozen = isFrozen;
        }
        [DebuggerStepThrough]
        static ESDictionary()
        {
            _splitByChars = SearchValues.Create(_joinBy.AsSpan());
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _byNames.GetEnumerator();
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool ContainsEnum(T key)
        {
            return _byValues.ContainsKey(key);
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool ContainsEnumByNumber(int number)
        {
            return _valuesAsInt.ContainsKey(number);
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool ContainsName([NotNullWhen(true)] string? name)
        {
            return !string.IsNullOrWhiteSpace(name) && _byNames.ContainsKey(name);
        }
        /// <inheritdoc/>
        public int CopyEnumsTo(Span<T> span)
        {
            if (span.Length < this.NameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "The provided span is too small to contain all the enum values.");
            }

            int written = 0;

            foreach (string key in _byNames.Keys)
            {
                span[written++] = _byNames[key];
            }

            return written;
        }
        /// <inheritdoc/>
        public int CopyNamesTo(Span<string> span)
        {
            if (span.Length < this.NameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "The provided span is too small to contain all the enum strings.");
            }

            int written = 0;
            foreach (string name in _byNames.Keys)
            {
                span[written++] = name;
            }

            return written;
        }

        /// <inheritdoc/>
        public bool IsDuplicated(T key, out int numberOfNames)
        {
            numberOfNames = 0;
            if (!_byValues.TryGetValue(key, out string? name)
                ||
                !name.AsSpan().ContainsAny(_splitByChars))
            {
                return false;
            }

            foreach (ReadOnlySpan<char> _ in name.SpanSplit(_joinBy))
            {
                numberOfNames++;
            }

            return numberOfNames > 0;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryGetEnum([NotNullWhen(true)] string name, out T value)
        {
            return _byNames.TryGetValue(name, out value);
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryGetEnum(int number, out T value)
        {
            return _valuesAsInt.TryGetValue(number, out value);
        }
        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryGetName(T key, [NotNullWhen(true)] out string? name)
        {
            return _byValues.TryGetValue(key, out name);
        }
        private static DictTuple CreateDictionaries(out int totalNameLength)
        {
            string[] names = Enum.GetNames<T>();
            bool hasDuplicates = false;
            totalNameLength = 0;

            Dictionary<T, string> enumToString = new(names.Length);
            Dictionary<string, T> stringToEnum = new(names.Length, StringComparer.InvariantCultureIgnoreCase);
            Dictionary<int, T> intValues = GetEnumValuesAsInt(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                totalNameLength += name.Length;

                T enumValue = Enum.Parse<T>(name);

                _ = stringToEnum.TryAdd(name, enumValue);
                if (!enumToString.TryAdd(enumValue, name))
                {
                    string existing = enumToString[enumValue];
                    enumToString[enumValue] = string.Concat(existing, _joinBy, name);
                    hasDuplicates = true;
                }
            }

            if (names.Length > 1)
            {
                totalNameLength += _joinBy.Length * (names.Length - 1);
            }

            return (enumToString, intValues, stringToEnum, hasDuplicates);
        }
        private static Dictionary<int, T> GetEnumValuesAsInt(int nameLength)
        {
            Type type = typeof(T);
            if (!typeof(int).Equals(Enum.GetUnderlyingType(type)))
            {
                return [];
            }

            Dictionary<int, T> dict = new(nameLength);

            int[] array = (int[])Enum.GetValuesAsUnderlyingType<T>();
            foreach (int value in array)
            {
                dict.TryAdd(value, GetIntAsEnum(value));
            }

            return dict;
        }
        private static T GetIntAsEnum(int value)
        {
            return Unsafe.As<int, T>(ref value);
        }
        [DebuggerStepThrough]
        private static bool TryGetDefaultName(in DictTuple dicts, [NotNullWhen(true)] out string? defaultName)
        {
            return dicts.EnumToString.TryGetValue(default, out defaultName);
        }

        /// <summary>Because I'm lazy :P</summary>
        [DebuggerStepThrough]
        private readonly struct DictTuple
        {
            internal readonly IDictionary<int, T> EnumAsInt { get; }
            internal readonly IDictionary<T, string> EnumToString { get; }
            internal readonly IDictionary<string, T> StringToEnum { get; }
            internal readonly bool HasDuplicates { get; }

            internal DictTuple(IDictionary<T, string> enumToString, IDictionary<int, T> enumAsInt, IDictionary<string, T> stringToEnum, bool hasDupes)
            {
                this.EnumAsInt = enumAsInt;
                this.EnumToString = enumToString;
                this.StringToEnum = stringToEnum;
                this.HasDuplicates = hasDupes;
            }

            internal (IReadOnlyDictionary<T, string>, IReadOnlyDictionary<string, T>, IReadOnlyDictionary<int, T>) ToReadOnly()
            {
                return (
                    new ReadOnlyDictionary<T, string>(this.EnumToString),
                    new ReadOnlyDictionary<string, T>(this.StringToEnum),
                    this.EnumAsInt.AsReadOnly());
            }
            internal (IReadOnlyDictionary<T, string>, IReadOnlyDictionary<string, T>, IReadOnlyDictionary<int, T>) ToFrozen()
            {
                return (
                    this.EnumToString.ToFrozenDictionary(),
                    this.StringToEnum.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase),
                    this.EnumAsInt.ToFrozenDictionary());
            }

            public static implicit operator DictTuple((IDictionary<T, string> EnumToString, IDictionary<int, T> EnumsAsInt, IDictionary<string, T> StringToEnum, bool HasDupes) tuple)
            {
                return new(tuple.EnumToString, tuple.EnumsAsInt, tuple.StringToEnum, tuple.HasDupes);
            }
        }
    }
}

