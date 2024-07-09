using System.Collections.Immutable;

namespace AD.Api.Core.Ldap;

public readonly partial struct DistinguishedName
{
    public readonly DistinguishedName Insert(int relativeNameIndex, ReadOnlySpan<char> value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(relativeNameIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(relativeNameIndex, this.Count);

        if (value.IsWhiteSpace())
        {
            return this;
        }

        if (!RelativeName.TryParseOne(value, out RelativeName toInsert))
        {
            throw new ArgumentException("The specified relative name does not have a valid attribute type.", nameof(value));
        }

        DistinguishedName @this = this;
        return @this.Insert(relativeNameIndex, toInsert);
    }
    public readonly DistinguishedName Insert(int relativeNameIndex, RelativeName relativeName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(relativeNameIndex, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(relativeNameIndex, this.Count);
        DistinguishedName @this = this;
        if (relativeName.IsEmpty)
        {
            return @this;
        }
        else if (@this.IsEmpty)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(relativeNameIndex, 0);
            return new DistinguishedName(new ReadOnlySpan<RelativeName>(ref relativeName));
        }

        ImmutableArray<RelativeName> array = @this._segments;
        int newLength = @this._length + relativeName.Value.Length + 1;

        return new DistinguishedName(array.Insert(relativeNameIndex, relativeName), in newLength);
    }
}