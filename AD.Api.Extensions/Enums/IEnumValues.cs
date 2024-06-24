using AD.Api.Attributes;

namespace AD.Api.Enums
{
    /// <summary>
    /// An interface for a read-only collection of <typeparamref name="TEnum"/> values that have 
    /// attributed members whose values can be read.
    /// </summary>
    /// <typeparam name="TEnum">The type of <see cref="Enum"/> this collection represents.</typeparam>
    /// <typeparam name="TAtt">
    ///     The type of <see cref="Attribute"/> which implements <see cref="IValuedAttribute{TValue}"/>
    ///     whose values are to be read and stored.
    /// </typeparam>
    /// <typeparam name="TValue">
    ///     The return type of the <see cref="IValuedAttribute{TValue}.Value"/> property of the
    ///     attributed members.
    /// </typeparam>
    public interface IEnumValues<TEnum, TAtt, TValue> : IEnumerable<TEnum>
        where TEnum : unmanaged, Enum
        where TAtt : Attribute, IValuedAttribute<TValue>
        where TValue : notnull
    {
        string this[TEnum key] { get; }

        int EnumCount { get; }
        IEnumStrings<TEnum> EnumStrings { get; }
        int ValueCount { get; }

        /// <inheritdoc cref="IEnumStrings{T}.ContainsEnum(T)"/>
        bool ContainsEnum(TEnum key);

        [return: NotNullIfNotNull(nameof(defaultValue))]
        TValue? GetValue(TEnum key, [AllowNull] TValue defaultValue = default);

        bool TryGetAttribute(TEnum key, [NotNullWhen(true)] out TAtt? attribute);
        /// <summary>
        /// Gets the attributed value of the <typeparamref name="T"/> enumeration that is associated with the 
        /// specified key.
        /// </summary>
        /// <param name="key">
        ///     The <typeparamref name="T"/> enumeration value to get the attribute value name from.
        /// </param>
        /// <param name="value">
        ///     When this method returns, contains the name(s) associated with the specified value, if the
        ///     name is found; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration is mapped with a value;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        bool TryGetValue(TEnum key, [NotNullWhen(true)] out TValue? value);
    }
}

