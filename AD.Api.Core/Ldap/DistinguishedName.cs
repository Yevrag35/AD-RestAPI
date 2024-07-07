using AD.Api.Collections.Enumerators;
using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Represents an LDAP distinguished name (DN) with the ability to split into its individual relative names.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DistinguishedName : IEnumerable<RelativeName>
{
    private static readonly char COMMA = CharConstants.COMMA;
    private readonly int _length;
    private readonly bool _notDefault;
    private readonly ImmutableArray<RelativeName> _segments;

    /// <summary>
    /// An empty distinguished name containing no relative names and of zero length.
    /// </summary>
    public static readonly DistinguishedName Empty = new(default);

    /// <summary>
    /// Gets the <see cref="RelativeName"/> component at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the <see cref="RelativeName"/> component to get.
    /// </param>
    /// <returns>
    /// The <see cref="RelativeName"/> component at the specified index.
    /// </returns>
    public ref readonly RelativeName this[int index] => ref _segments.AsSpan()[index];

    /// <summary>
    /// Gets the number of <see cref="RelativeName"/> components in the distinguished name.
    /// </summary>
    public readonly int Count => _segments.Length;
    /// <summary>
    /// Indicates whether the distinguished name is empty or default-initialized.
    /// </summary>
    public readonly bool IsEmpty => !_notDefault && _segments.IsEmpty;
    /// <summary>
    /// Gets the <see cref="string"/> length of the entire distinguished name.
    /// </summary>
    public readonly int Length => _length;

    private DistinguishedName(ImmutableArray<RelativeName> segments, in int length)
    {
        _notDefault = true;
        _length = length;
        _segments = segments;
    }
    public DistinguishedName(ReadOnlySpan<RelativeName> segments)
    {
        _notDefault = true;
        if (segments.IsEmpty)
        {
            _segments = [];
            _length = 0;
        }
        else
        {
            _segments = ImmutableArray.Create(segments);
            _length = GetTotalLength(segments);
        }
    }

    public readonly ImmutableArray<RelativeName> AsImmutableArray()
    {
        return !this.IsEmpty ? _segments : [];
    }
    public readonly ReadOnlySpan<RelativeName> AsSpan()
    {
        return _segments.AsSpan();
    }
    public readonly ReadOnlySpan<RelativeName> AsSpan(int start)
    {
        return !this.IsEmpty ? _segments.AsSpan(start, _segments.Length - start) : [];
    }
    public readonly ReadOnlySpan<RelativeName> AsSpan(int start, int length)
    {
        return !this.IsEmpty ? _segments.AsSpan(start, length) : [];
    }
    public readonly int CopyTo(Span<char> destination)
    {
        return CopyTo(_segments.AsSpan(), destination);
    }
    public readonly int CopyTo(Span<char> destination, int relativeNameIndex)
    {
        return this.CopyTo(destination, relativeNameIndex, this.Count - relativeNameIndex);
    }
    public readonly int CopyTo(Span<char> destination, int relativeNameIndex, int relativeNameLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, this.Length, nameof(destination));
        return CopyTo(_segments.AsSpan(relativeNameIndex, relativeNameLength), destination);
    }
    public ArrayRefEnumerator<RelativeName> GetEnumerator()
    {
        ReadOnlySpan<RelativeName> span = _segments.AsSpan();
        return new ArrayRefEnumerator<RelativeName>(span);
    }
    readonly IEnumerator<RelativeName> IEnumerable<RelativeName>.GetEnumerator()
    {
        ImmutableArray<RelativeName> segs = _segments;
        return new ArrayEnumerator<RelativeName>(segs.AsSpan());
    }
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<RelativeName>)this).GetEnumerator();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public readonly ReadOnlySpan<RelativeName> GetParentSegments()
    {
        if (this.IsEmpty || _segments.Length <= 1)
        {
            return [];
        }

        return _segments.AsSpan(1, _segments.Length - 1);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public readonly string GetParent()
    {
        ReadOnlySpan<RelativeName> parentSegments = this.GetParentSegments();
        if (parentSegments.IsEmpty)
        {
            return string.Empty;
        }
        else if (parentSegments.Length == 1)
        {
            return parentSegments[0].Value;
        }

        ref readonly RelativeName first = ref _segments.AsSpan(0, 1)[0];
        int length = _length - first.Value.Length - 1;
        return ToString(parentSegments, in length);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public readonly DistinguishedName ToParent()
    {
        if (this.IsEmpty || _segments.Length <= 1)
        {
            return Empty;
        }

        ref readonly RelativeName first = ref this[0];
        if (first.AttributeType == RelativeNameType.DomainComponent)
        {
            return Empty;
        }

        int length = _length - first.Value.Length - 1;
        return new(ImmutableArray.Create(_segments, 1, _segments.Length - 1), in length);
    }

    /// <summary>
    /// Returns the string representation of the full distinguished name.
    /// </summary>
    /// <returns>The string representation of the full distinguished name.</returns>
    public override readonly string ToString()
    {
        return ToString(_segments.AsSpan(), in _length);
    }

    #region STATIC METHODS
    /// <summary>
    /// Counts the number of <see cref="RelativeName"/> components in the provided span of characters if it were
    /// to be split.
    /// </summary>
    /// <param name="path">
    /// The span of characters to count the number of <see cref="RelativeName"/> components in.
    /// </param>
    /// <returns>
    /// The number of <see cref="RelativeName"/> components that would make up the distinguished name if parsed.
    /// </returns>
    public static int CountNumberOfRelativeNames(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
        {
            return 0;
        }

        int count = 1;
        for (int i = 0; i < path.Length; i++)
        {
            if (COMMA == path[i] && !path.IsEscapedAt(in i))
            {
                count++;
            }
        }

        return count;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="segments"></param>
    /// <returns></returns>
    public static string ToString(ReadOnlySpan<RelativeName> segments)
    {
        if (segments.IsEmpty)
        {
            return string.Empty;
        }
        else if (segments.Length == 1)
        {
            return segments[0].Value;
        }

        int length = GetTotalLength(segments);
        return ToString(segments, in length);
    }

    #endregion
}
