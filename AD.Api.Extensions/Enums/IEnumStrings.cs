namespace AD.Api.Enums
{
    public interface IEnumStrings
    {
        /// <summary>
        /// Gets the number of <see cref="Enum"/> names in this collection.
        /// </summary>
        /// <remarks>
        ///     When <see cref="HasDuplicates"/> is <see langword="true"/>, this value may be more than 
        ///     <see cref="EnumCount"/>. Therefore, if you need to know the traditional "count" of this 
        ///     collection, always use this property.
        /// </remarks>
        int NameCount { get; }

        /// <summary>
        /// Copies all the enumeration names to the specified span.
        /// </summary>
        /// <param name="span">The span to copy the names to.</param>
        /// <returns>The number of names written to <paramref name="span"/>.</returns>
        int CopyNamesTo(Span<string> span);
    }

    /// <summary>
    /// An interface for a read-only collection of <see cref="Enum"/> names and their values that can be 
    /// looked up by either.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> type for this collection.</typeparam>
    public interface IEnumStrings<T> : IEnumStrings, IReadOnlyCollection<KeyValuePair<string, T>> where T : unmanaged, Enum
    {
        /// <summary>
        /// Gets the name of the <typeparamref name="T"/> enumeration that is associated with the
        /// specified key.
        /// </summary>
        /// <param name="key">The enumeration value key to search for.</param>
        /// <returns>
        /// The name of the <typeparamref name="T"/> enumeration that is associated with the specified key.
        /// </returns>
        string this[T key] { get; }

        /// <summary>
        /// The default name for <typeparamref name="T"/> if the enumeration has a 0 value.
        /// </summary>
        string? DefaultName { get; }
        /// <summary>
        /// Gets the number of <typeparamref name="T"/> enumeration values in this collection.
        /// </summary>
        /// <remarks>
        ///     When <see cref="HasDuplicates"/> is <see langword="true"/>, this value may be less than 
        ///     <see cref="NameCount"/>. Therefore, if you need to know the traditional "count" of this 
        ///     collection, do not use this property. Instead, use <see cref="NameCount"/>.
        /// </remarks>
        int EnumCount { get; }
        /// <summary>
        /// Indicates whether the <typeparamref name="T"/> enumeration has a default name.
        /// </summary>
        /// <remarks>
        /// When <see langword="true"/>, the <see cref="DefaultName"/> property will return a 
        /// non-null value.
        /// </remarks>
        [MemberNotNullWhen(true, nameof(DefaultName))]
        bool HasDefaultName { get; }
        /// <summary>
        /// Indicates whether the <typeparamref name="T"/> enumeration has duplicate values with different
        /// names.
        /// </summary>
        /// <remarks>
        ///     When this <see langword="true"/> and looking up elements by value, the name returned may be 
        ///     a concatenation of all the names with that value.
        /// </remarks>
        bool HasDuplicates { get; }
        /// <summary>
        /// Indicates whether the <typeparamref name="T"/> enumeration is based on <see cref="int"/>.
        /// </summary>
        bool IsIntegerBased { get; }
        /// <summary>
        /// Indicates whether the <typeparamref name="T"/> enumeration is a flags enumeration.
        /// </summary>
        bool IsFlagsEnum { get; }
        /// <summary>
        /// Indicates whether this collection has been frozen and not just read-only.
        /// </summary>
        bool IsFrozen { get; }

        int TotalNameLength { get; }

        IEnumerable<T> Values { get; }

        /// <summary>
        /// Returns whether the <typeparamref name="T"/> enumeration contains the specified value.
        /// </summary>
        /// <param name="key">
        ///     The enumeration value to search for.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration has the specified value 
        ///     defined; otherwise, <see langword="false"/>.
        /// </returns>
        bool ContainsEnum(T key);
        /// <summary>
        /// Returns whether the <typeparamref name="T"/> enumeration contains the specified interger value.
        /// </summary>
        /// <param name="number">The enumeration integer to search for.</param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration contains the specified number
        ///     as a value; otherwise, <see langword="false"/>.
        /// </returns>
        bool ContainsEnumByNumber(int number);
        /// <summary>
        /// Returns whether the <typeparamref name="T"/> enumeration contains the specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the <typeparamref name="T"/> enumeration to search for.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration has the specified name 
        ///     defined; otherwise, <see langword="false"/>.
        /// </returns>
        bool ContainsName([NotNullWhen(true)] string? name);

        /// <summary>
        /// Copies all the <typeparamref name="T"/> enumeration names to the specified <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="span">The span to copy the names to.</param>
        /// <returns>The number of names written to the <paramref name="span"/>.</returns>
        int CopyEnumsTo(Span<T> span);

        /// <summary>
        /// Returns whether the <typeparamref name="T"/> enumeration contains the specified value and
        /// whether it is duplicated.
        /// </summary>
        /// <param name="key">The enumeration value to search for.</param>
        /// <param name="numberOfNames">
        ///     When this method returns, contains the number of names the value was matched to.
        ///     If the value is not found, this will be 0. When this method returns <see langword="true"/>,
        ///     it can be expected that this value will be greater than 1.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration has the specified value
        ///     and is duplicated; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsDuplicated(T key, out int numberOfNames);
        /// <summary>
        /// Gets the value of the <typeparamref name="T"/> enumeration that is associated with the 
        /// specified name.
        /// </summary>
        /// <param name="name">
        ///     The name of the <typeparamref name="T"/> enumeration to get the value of.
        /// </param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified name, if the
        ///     name is found; otherwise, the default value for the type of the <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration contains an value with
        ///     the specified name; otherwise, <see langword="false"/>.
        /// </returns>
        bool TryGetEnum([NotNullWhen(true)] string name, out T value);
        /// <summary>
        /// Gets the value of the <typeparamref name="T"/> enumeration that is associated with the 
        /// specified <see cref="int"/> value.
        /// </summary>
        /// <param name="name">
        ///     The <see cref="int"/> value of the <typeparamref name="T"/> enumeration to get the value of.
        /// </param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified integer value, if the
        ///     value is found; otherwise, the default value for the type of the <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration contains an value with
        ///     the specified <see cref="int"/> value; otherwise, <see langword="false"/>. <see langword="false"/> is
        ///     also returned is <typeparamref name="T"/> is not based on <see cref="int"/>.
        /// </returns>
        bool TryGetEnum(int number, out T value);
        /// <summary>
        /// Gets the value of the <typeparamref name="T"/> enumeration that is associated with the 
        /// specified name.
        /// </summary>
        /// <param name="value">
        ///     The value of the <typeparamref name="T"/> enumeration to get the name of.
        /// </param>
        /// <param name="name">
        ///     When this method returns, contains the name(s) associated with the specified value, if the
        ///     name is found; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <typeparamref name="T"/> enumeration contains name(s) with
        ///     the specified value; otherwise, <see langword="false"/>.
        /// </returns>
        bool TryGetName(T value, [NotNullWhen(true)] out string? name);
    }
}

