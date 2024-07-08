
namespace AD.Api.Core.Ldap;

public readonly partial struct RelativeName
{
    public bool Equals(RelativeName other)
    {
        if (this.IsEmpty)
        {
            return other.IsEmpty;
        }
        else if (other.IsEmpty || this.AttributeType != other.AttributeType)
        {
            return false;
        }

        return this.GetName().Equals(other.GetName(), StringComparison.OrdinalIgnoreCase);
    }
    public bool Equals(string? other)
    {
        return this.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
    public bool Equals(string? other, StringComparison comparisonType)
    {
        if (string.IsNullOrEmpty(other))
        {
            return this.IsEmpty;
        }
        else if (this.IsEmpty)
        {
            return false;
        }

        return _value.Equals(other, comparisonType);
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is RelativeName name && this.Equals(name))
            || (obj is string str && this.Equals(str));
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(this.AttributeType, StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value));
    }

    public static bool operator ==(RelativeName left, RelativeName right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(RelativeName left, RelativeName right)
    {
        return !(left == right);
    }
    public static bool operator ==(RelativeName left, string? right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(RelativeName left, string? right)
    {
        return !(left == right);
    }
    public static bool operator ==(string? left, RelativeName right)
    {
        return right.Equals(left);
    }
    public static bool operator !=(string? left, RelativeName right)
    {
        return !(left == right);
    }
}

