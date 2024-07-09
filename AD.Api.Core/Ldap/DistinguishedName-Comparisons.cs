
namespace AD.Api.Core.Ldap
{
    public readonly partial struct DistinguishedName
    {
        public int CompareTo(DistinguishedName other)
        {
            if (this.IsEmpty)
            {
                return other.IsEmpty ? 0 : -1;
            }
            else if (other.IsEmpty)
            {
                return 1;
            }

            int howMany = Math.Min(this.Count, other.Count);
            int compare = 0;
            for (int i = 0; i < howMany; i++)
            {
                ref readonly RelativeName mine = ref this[i];
                ref readonly RelativeName theirs = ref other[i];
                compare = CompareSection(in mine, in theirs);
                if (compare != 0)
                {
                    break;
                }
            }

            return compare == 0
                ? this.Count.CompareTo(other.Count)
                : compare;
        }
        private static int CompareSection(ref readonly RelativeName mine, ref readonly RelativeName other)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(mine.Value, other.Value);
        }
        public bool Equals(DistinguishedName other)
        {
            DistinguishedName @this = this;
            if (@this.IsEmpty)
            {
                return other.IsEmpty;
            }
            else if (other.IsEmpty || @this.Length != other.Length || @this.Count != other.Count)
            {
                return false;
            }

            int count = @this.Count;
            for (int i = 0; i < count; i++)
            {
                ref readonly RelativeName mine = ref @this[i];
                ref readonly RelativeName theirs = ref other[i];
                if (!mine.Equals(theirs))
                {
                    return false;
                }
            }

            return true;
        }
        public bool Equals(string? other)
        {
            return this.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        public bool Equals(string? other, StringComparison comparisonType)
        {
            DistinguishedName @this = this;
            if (@this.IsEmpty)
            {
                return string.IsNullOrEmpty(other);
            }
            else if (string.IsNullOrEmpty(other) || @this.Length != other.Length)
            {
                return false;
            }

            Span<char> chars = stackalloc char[@this.Length];
            int written = @this.CopyTo(chars);
            ReadOnlySpan<char> span = chars.Slice(0, written);
            return span.Equals(other, comparisonType);
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is DistinguishedName dn ? this.Equals(dn) : obj is string str && this.Equals(str);
        }
        public override int GetHashCode()
        {
            DistinguishedName @this = this;
            if (@this.IsEmpty)
            {
                return 0;
            }
            else if (@this.Count == 1)
            {
                ref readonly RelativeName first = ref @this.GetFirst();
                return StringComparer.OrdinalIgnoreCase.GetHashCode(first.Value);
            }

            int hash = 0;
            foreach (RelativeName name in @this)
            {
                unchecked
                {
                    hash += StringComparer.OrdinalIgnoreCase.GetHashCode(name.Value);
                }
            }

            return hash;
        }

        public static bool operator ==(DistinguishedName x, DistinguishedName y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(DistinguishedName x, DistinguishedName y)
        {
            return !(x == y);
        }
        public static bool operator ==(DistinguishedName x, string? y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(DistinguishedName x, string? y)
        {
            return !(x == y);
        }
        public static bool operator ==(string? x, DistinguishedName y)
        {
            return y == x;
        }
        public static bool operator !=(string? x, DistinguishedName y)
        {
            return !(x == y);
        }
    }
}
