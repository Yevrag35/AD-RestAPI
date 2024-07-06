using System.Runtime.InteropServices;

namespace AD.Api.Components;

[DebuggerStepThrough]
[StructLayout(LayoutKind.Auto)]
public readonly struct LdapBoolean : 
    IComparable<bool>,
    IComparable<LdapBoolean>,
    IEquatable<bool>,
    IEquatable<LdapBoolean>,
    IEquatable<string>
{
    public const string FalseString = "FALSE";
    public const string TrueString = "TRUE";

    private readonly string? _strValue;
    private readonly bool _value;

    public string AsString => _strValue ?? FalseString;

    private LdapBoolean(bool value, [ConstantExpected] string strValue)
    {
        _strValue = strValue;
        _value = value;
    }

    public static readonly LdapBoolean False = new(false, FalseString);
    public static readonly LdapBoolean True = new(true, TrueString);

    public readonly int CompareTo(LdapBoolean other)
    {
        return _value.CompareTo(other._value);
    }
    public readonly int CompareTo(bool other)
    {
        return _value.CompareTo(other);
    }
    public readonly bool Equals(LdapBoolean other)
    {
        return _value == other._value;
    }
    public readonly bool Equals(bool other)
    {
        return _value == other;
    }
    public readonly bool Equals([NotNullWhen(true)] string? other)
    {
        return this.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
    public readonly bool Equals([NotNullWhen(true)] string? other, StringComparison comparisonType)
    {
        ReadOnlySpan<char> span = other;
        ReadOnlySpan<char> strVal = this.AsString;
        return (span.Length == TrueString.Length || span.Length == FalseString.Length)
            &&
                (span.Equals(strVal, comparisonType)
                ||
                span.Equals(strVal, comparisonType)
                );
    }
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is LdapBoolean boolean && _value == boolean._value)
            || (obj is bool b && _value == b);
    }
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_value);
    }

    public static bool ParseBool(ReadOnlySpan<char> span)
    {
        return span.Equals(TrueString, StringComparison.OrdinalIgnoreCase);
    }
    public static bool TryParseBool(ReadOnlySpan<char> span, out bool value)
    {
        value = false;
        if (span.IsWhiteSpace())
        {
            return false;
        }
        else if (span.Equals(TrueString, StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }
        else if (span.Equals(FalseString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a string that represents the LDAP boolean value.
    /// </summary>
    /// <returns>
    /// The string representation of the <see cref="LdapBoolean"/> value.  <c>TRUE</c> if <see langword="true"/> or
    /// <c>FALSE</c> if <see langword="false"/>.
    /// </returns>
    public override readonly string ToString()
    {
        return this.AsString;
    }

    public static implicit operator bool(LdapBoolean value)
    {
        return value._value;
    }
    public static implicit operator LdapBoolean(bool value)
    {
        return value switch
        {
            true => True,
            false => False,
        };
    }
    public static implicit operator string(LdapBoolean value)
    {
        return value.AsString;
    }

    public static bool operator ==(LdapBoolean left, LdapBoolean right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(LdapBoolean left, LdapBoolean right)
    {
        return !(left == right);
    }
    public static bool operator ==(LdapBoolean left, bool right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(LdapBoolean left, bool right)
    {
        return !(left == right);
    }
    public static bool operator ==(bool left, LdapBoolean right)
    {
        return right.Equals(left);
    }
    public static bool operator !=(bool left, LdapBoolean right)
    {
        return !(left == right);
    }
    public static bool operator ==(LdapBoolean left, string? right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(LdapBoolean left, string? right)
    {
        return !(left == right);
    }
    public static bool operator ==(string? left, LdapBoolean right)
    {
        return right.Equals(left);
    }
    public static bool operator !=(string? left, LdapBoolean right)
    {
        return !(left == right);
    }
}
