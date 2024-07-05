using AD.Api.Attributes;
using AD.Api.Attributes.Services;
using AD.Api.Enums;
using AD.Api.Spans;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Typical Relative Distinguished Name (RDN) attribute types.
/// </summary>
public enum RelativeNameType
{
    /// <summary>
    /// The common name attribute: <c>CN</c>
    /// </summary>
    /// <remarks>
    /// Also used as the default when no attribute type is specified.
    /// </remarks>
    [BackendValue("CN=")]
    CommonName = 0x0,
    /// <summary>
    /// The organizational unit name attribute: <c>OU</c>
    /// </summary>
    [BackendValue("OU=")]
    OrganizationalUnit = 0x1,
    /// <summary>
    /// The domain component attribute: <c>DC</c>
    /// </summary>
    [BackendValue("DC=")]
    DomainComponent = 0x2,
    /// <summary>
    /// The organization name attribute: <c>O</c>
    /// </summary>
    [BackendValue("O=")]
    Organization = 0x3,
    /// <summary>
    /// The street address attribute: <c>STREET</c>
    /// </summary>
    [BackendValue("STREET=")]
    StreetAddress = 0x4,
    /// <summary>
    /// The locality name attribute: <c>L</c>
    /// </summary>
    [BackendValue("L=")]
    Locality = 0x5,
    /// <summary>
    /// The state or province name attribute: <c>ST</c>
    /// </summary>
    [BackendValue("ST=")]
    StateOrProvince = 0x6,
    /// <summary>
    /// The country name attribute: <c>C</c>
    /// </summary>
    [BackendValue("C=")]
    Country = 0x7,
    /// <summary>
    /// The user ID attribute: <c>UID</c>
    /// </summary>
    [BackendValue("UID=")]
    UserId = 0x8,
}

/// <summary>
/// A struct representing one relative distinguished name that when combined and separated with commas forms
/// a fully-qualified distinguished name.
/// </summary>
/// <remarks>
/// Also known as a Relative Distinguished Name (RDN), a relative name is a single attribute-value pair that
/// represents a single attribute of an object in a directory.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[DynamicDependencyRegistration]
public readonly struct RelativeName
{
    private const int DN_SPAN_LIMIT = 256;  // Maximum stackalloc length of a distinguished name.
    private const int MINIMUM_NAME_INDEX = 3;   // Minimum index for a valid attributed name.
    /// <summary>
    /// A read-only dictionary of the <see cref="RelativeNameType"/> attribute values and their LDAP string 
    /// representations.
    /// </summary>
    /// <remarks>
    /// The <see cref="string"/> values include the trailing, separating equals sign, but do not include the preceding
    /// comma. An example:
    /// <code>
    /// { RelativeNameType.CommonName, "CN=" }
    /// </code>
    /// </remarks>
    public static readonly IEnumValues<RelativeNameType, BackendValueAttribute, string> AttributeStrings;
    private static readonly FrozenDictionary<string, RelativeNameType> _attributeValues;
    public static readonly RelativeName Empty;
    public static readonly SearchValues<char> UniqueAttributeChars;
    static RelativeName()
    {
        Empty = new(RelativeNameType.CommonName, string.Empty, -1);

        AttributeStrings = EnumValues.Create<RelativeNameType, BackendValueAttribute, string>(freeze: true);
        Dictionary<string, RelativeNameType> valueDict = AttributeStrings
            .ToValueDictionary(StringComparer.OrdinalIgnoreCase);

        _attributeValues = FrozenDictionary.ToFrozenDictionary(valueDict, valueDict.Comparer);
        Span<char> chars = stackalloc char[AttributeStrings.ValueCount * 7];
        int count = 0;
        foreach (char c in _attributeValues.Keys.SelectMany(x => x).Distinct())
        {
            chars[count++] = c;
        }

        UniqueAttributeChars = SearchValues.Create(chars.Slice(0, count));
    }

    private readonly int _nameStartIndex;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly bool _notEmpty;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string? _value;

    public readonly RelativeNameType AttributeType { get; }
    [MemberNotNullWhen(false, nameof(_value))]
    public readonly bool IsEmpty => !_notEmpty;
    public readonly string Value => _value ?? string.Empty;

    private RelativeName(RelativeNameType attributeType, string value, in int nameIndex)
    {
        this.AttributeType = attributeType;
        if (!string.IsNullOrWhiteSpace(value))
        {
            _value = value;
            _notEmpty = nameIndex >= 2 && nameIndex < value.Length - 1;
            _nameStartIndex = nameIndex;
        }
        else
        {
            _value = string.Empty;
            _nameStartIndex = -1;
            _notEmpty = false;
        }
    }

    private static ReadOnlySpan<char> BuildPrefix(in RelativeNameType nameTypeIfNotPresent, Span<char> buffer, ReadOnlySpan<char> span, ref ReadOnlySpan<char> prefix)
    {
        prefix = AttributeStrings.GetValue(nameTypeIfNotPresent);
        prefix.CopyTo(buffer);
        span.CopyTo(buffer.Slice(prefix.Length));
        return buffer.Slice(0, span.Length + prefix.Length);
    }
    public ReadOnlySpan<char> GetName()
    {
        return _notEmpty ? _value.AsSpan(_nameStartIndex) : [];
    }
    private static RelativeNameType GetRelativeNameType(ReadOnlySpan<char> prefix)
    {
        ThrowWhenInvalidPrefix(prefix);

        foreach (var kvp in _attributeValues)
        {
            if (prefix.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        Debug.Fail("This should be unreachable...");
        return RelativeNameType.CommonName;
    }

    private static bool TryGetRelativeNameType(ReadOnlySpan<char> prefix, out RelativeNameType result)
    {
        if (prefix.Length >= 2 && prefix.Length <= 7 && !prefix.ContainsAnyExcept(UniqueAttributeChars))
        {
            foreach (var kvp in _attributeValues)
            {
                if (prefix.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    result = kvp.Value;
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
    private static bool IsValidPrefix(ReadOnlySpan<char> prefix, [NotNullWhen(false)] out ArgumentException? exception)
    {
        if (prefix.Length < 2 || prefix.Length > 7)
        {
            exception = new ArgumentException("No valid attribute type found is less than 2 or more than 7 characters in length.", nameof(prefix));
            return false;
        }
        else if (prefix.ContainsAnyExcept(UniqueAttributeChars))
        {
            exception = new ArgumentException($"The specified attribute type contains invalid characters: {prefix.ToString()}", nameof(prefix));
            return false;
        }

        exception = null;
        return true;
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="value"></param>
    ///// <param name="nameTypeIfNotPresent"></param>
    ///// <returns></returns>
    ///// <exception cref="ArgumentException"/>
    //public static RelativeName ParseOne(ReadOnlySpan<char> value, RelativeNameType nameTypeIfNotPresent = RelativeNameType.CommonName)
    //{
    //    scoped ReadOnlySpan<char> span = value;
    //    if (span.IsWhiteSpace())
    //    {
    //        return Empty;
    //    }

    //    char[]? array = null;
    //    bool isRented = false;

    //    int index = span.IndexOf('=');
    //    scoped ReadOnlySpan<char> prefix;
    //    if (index + 1 >= span.Length)
    //    {
    //        return Empty;
    //    }
        
    //    if (index < MINIMUM_NAME_INDEX)
    //    {
    //        int length = span.Length + 3;
    //        Span<char> buffer = length < DN_SPAN_LIMIT
    //            ? stackalloc char[length]
    //            : SpanExtensions.RentArray(in length, ref isRented, ref array);

    //        prefix = default;
    //        span = BuildPrefix(in nameTypeIfNotPresent, buffer, span, ref prefix);
    //    }
    //    else
    //    {
    //        prefix = span.Slice(0, index);
    //        nameTypeIfNotPresent = GetRelativeNameType(prefix);
    //    }

    //    RelativeName result = new(nameTypeIfNotPresent, span.ToString(), index);

    //    if (isRented)
    //    {
    //        ArrayPool<char>.Shared.Return(array!);
    //    }

    //    return result;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryParseOne(ReadOnlySpan<char> value, out RelativeName result)
    {
        scoped ReadOnlySpan<char> span = value;
        if (span.IsWhiteSpace())
        {
            result = Empty;
            return false;
        }

        RelativeNameType nameTypeIfNotPresent = RelativeNameType.CommonName;
        char[]? array = null;
        bool isRented = false;

        int index = span.IndexOf('=') + 1;
        scoped ReadOnlySpan<char> prefix;
        if (index >= span.Length)
        {
            result = Empty;
            return false;
        }

        if (index < MINIMUM_NAME_INDEX)
        {
            int length = span.Length + 3;
            Span<char> buffer = length < DN_SPAN_LIMIT
                ? stackalloc char[length]
                : SpanExtensions.RentArray(in length, ref isRented, ref array);

            prefix = default;
            span = BuildPrefix(in nameTypeIfNotPresent, buffer, span, ref prefix);
        }
        else
        {
            prefix = span.Slice(0, index);
            if (!TryGetRelativeNameType(prefix, out nameTypeIfNotPresent))
            {
                result = Empty;
                return false;
            }
        }

        result = new(nameTypeIfNotPresent, span.ToString(), index);
        if (isRented)
        {
            ArrayPool<char>.Shared.Return(array!);
        }

        return true;
    }

    /// <summary>
    /// Returns the attributed string representation of the relative name.
    /// </summary>
    /// <returns>
    /// The attributed string representation of the relative name in the format with no separating commas: 
    /// <c>AttributeType=Name</c>.
    /// </returns>
    public override string ToString()
    {
        return this.Value;
    }

    public static void ThrowWhenInvalidPrefix(ReadOnlySpan<char> prefix)
    {
        if (prefix.IsEmpty)
        {
            return;
        }

        if (!IsValidPrefix(prefix, out ArgumentException? exception))
        {
            throw exception;
        }
    }

    [DynamicDependencyRegistrationMethod]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private static void AddToServices(IServiceCollection services)
    {
        services.AddSingleton(AttributeStrings)
                .AddSingleton(AttributeStrings.EnumStrings);
    }
}

