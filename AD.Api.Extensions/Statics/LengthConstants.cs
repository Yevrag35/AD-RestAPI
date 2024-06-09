using System.Collections.Frozen;
using System.Numerics;

namespace AD.Api.Statics
{
    /// <summary>
    /// A <see langword="static"/> class containing constants for various maximum character lengths of data types.
    /// </summary>
    public static class LengthConstants
    {
        // NUMERICAL LENGTHS
        /// <summary>
        /// Maximum character length of a <see cref="byte"/>.
        /// </summary>
        public const int BYTE_MAX = 3;
        /// <summary>
        /// Maximum character length of a <see cref="short"/>.
        /// </summary>
        public const int SHORT_MAX = 6;
        /// <summary>
        /// Maximum character length of a <see cref="int"/>.
        /// </summary>
        public const int INT_MAX = 11;
        /// <summary>
        /// Maximum character length of a <see cref="long"/>.
        /// </summary>
        public const int LONG_MAX = 20;
        /// <summary>
        /// Maximum character length of a <see cref="uint"/>.
        /// </summary>
        public const int UINT_MAX = INT_MAX - 1;
        /// <summary>
        /// Maximum character length of a <see cref="ulong"/>.
        /// </summary>
        public const int ULONG_MAX = LONG_MAX;
        /// <summary>
        /// Maximum character length of a <see cref="double"/>.
        /// </summary>
        public const int DOUBLE_MAX = 24;
        /// <summary>
        /// Maximum character length of a <see cref="decimal"/>.
        /// </summary>
        public const int DECIMAL_MAX = 30;
        /// <summary>
        /// Maximum character length of an <see cref="Int128"/>.
        /// </summary>
        public const int INT128_MAX = 40;
        /// <summary>
        /// Maximum character length of a <see cref="float"/>.
        /// </summary>
        /// <remarks>
        /// Equivalent to:
        /// <para><c>float.MinValue.ToString("N").Length</c></para> ...which includes 2 decimal places.
        /// </remarks>
        public const int FLOAT_MAX = 55;

        // GUID LENGTHS
        /// <summary>
        /// Maximum character length of a <see cref="Guid"/> in the "B" or "P" formats.
        /// </summary>
        public const int GUID_FORM_B_OR_P = 38;
        /// <summary>
        /// Maximum character length of a <see cref="Guid"/> in the "D" format.
        /// </summary>
        public const int GUID_FORM_N = 32;
        /// <summary>
        /// Maximum character length of a <see cref="Guid"/> in the "D" format.
        /// </summary>
        public const int GUID_FORM_D = 36;
        /// <summary>
        /// Maximum character length of a <see cref="Guid"/> in the "X" format.
        /// </summary>
        public const int GUID_FORM_X = 68;

        private static readonly FrozenDictionary<Type, int> _byTypeLookup;

        static LengthConstants()
        {
            Dictionary<Type, int> lookup = new(10)
            {
                { typeof(byte), BYTE_MAX },
                { typeof(int), INT_MAX },
                { typeof(long), LONG_MAX },
                { typeof(uint), UINT_MAX },
                { typeof(ulong), ULONG_MAX },
                { typeof(double), DOUBLE_MAX },
                { typeof(decimal), DECIMAL_MAX },
                { typeof(float), FLOAT_MAX },
                { typeof(Guid), GUID_FORM_D },      // Default GUID format
                { typeof(Int128), INT128_MAX },
            };

            _byTypeLookup = lookup.ToFrozenDictionary();
        }

        /// <inheritdoc cref="TryGetLength(Type, out int)"/>
        /// <typeparam name="T">
        ///     <inheritdoc cref="TryGetLength(Type, out int)" path="/param[1]"/>
        /// </typeparam>
        public static bool TryGetLength<T>(out int length)
        {
            return TryGetLength(typeof(T), out length);
        }
        /// <summary>
        /// Attempts to get the maximum character length for the specified type.
        /// </summary>
        /// <param name="type">
        /// The type to get the maximum length for.
        /// </param>
        /// <param name="length">
        /// When this method returns, contains the maximum length for the specified type, if the type is found;
        /// otherwise, 0.
        /// </param>
        /// <returns>
        /// The maximum character length for the specified type, if the type is found; otherwise, 0.
        /// </returns>
        public static bool TryGetLength(Type type, out int length)
        {
            return _byTypeLookup.TryGetValue(type, out length);
        }
    }
}

