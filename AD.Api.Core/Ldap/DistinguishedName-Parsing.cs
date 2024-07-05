using AD.Api.Spans;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using AD.Api.Strings.Spans;
using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Ldap;

public sealed partial class DistinguishedName
{
    static readonly char COMMA = CharConstants.COMMA;

    /// <summary>
    /// Splits the current <see cref="DistinguishedName"/> into its constituent <see cref="RelativeName"/> components.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"/>
    public RelativeName[] Split()
    {
        return this.IsConstructed
            ? Split(_fullValue)
            : Split(this.ToString());
    }
    /// <summary>
    /// Attempts to split the current <see cref="DistinguishedName"/> into its constituent <see cref="RelativeName"/> 
    /// components and write them to the provided span.
    /// </summary>
    /// <param name="destination">The span to write the <see cref="RelativeName"/> parts to.</param>
    /// <param name="namesWritten">
    /// When this method returns, contains the number of <see cref="RelativeName"/> parts written to 
    /// <paramref name="destination"/>.
    /// </param>
    /// <returns></returns>
    public bool TrySplit(Span<RelativeName> destination, out int namesWritten)
    {
        return this.IsConstructed
            ? TrySplit(_fullValue, destination, out namesWritten)
            : TrySplit(this.ToString(), destination, out namesWritten);
    }

    public static DistinguishedName Join(ReadOnlySpan<RelativeName> relativeNames)
    {
        if (relativeNames.IsEmpty)
        {
            return new();
        }
        else if (relativeNames.Length == 1)
        {
            return new(relativeNames[0].Value);
        }

        int length = GetLength(relativeNames) + relativeNames.Length - 1;
        char[]? array = null;
        bool isRented = false;
        Span<char> span = length < MAX_LENGTH
            ? stackalloc char[length]
            : SpanExtensions.RentArray(in length, ref isRented, ref array);

        ref readonly RelativeName first = ref relativeNames[0];
        first.Value.CopyTo(span);
        span[first.Value.Length] = COMMA;
        int pos = first.Value.Length + 1;

        relativeNames = relativeNames.Slice(1);
        if (relativeNames.Length >= 2)
        {
            foreach (RelativeName name in relativeNames.Slice(0, relativeNames.Length - 1))
            {
                name.Value.CopyToSlice(span, ref pos);
                span[pos++] = COMMA;
            }
        }

        relativeNames[^1].Value.CopyToSlice(span, ref pos);
        int minusFirst = pos - first.Value.Length - 1;

        DistinguishedName result = new(span.Slice(0, pos), first.Value, span.Slice(first.Value.Length + 1, minusFirst));
        
        if (isRented)
        {
            ArrayPool<char>.Shared.Return(array!);
        }

        return result;
    }

    private static int GetLength(ReadOnlySpan<RelativeName> span)
    {
        int length = 0;
        foreach (RelativeName name in span)
        {
            length += name.Value.Length;
        }

        return length;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"/>
    public static RelativeName[] Split(ReadOnlySpan<char> path)
    {
        SpanCharArray list = new(path.Length, COMMA);

        int start = 0;
        int i = 0;
        for (i = 0; i < path.Length; i++)
        {
            if (COMMA == path[i] && !path.IsEscapedAt(in i))
            {
                list.Add(path.Slice(start, i - start));
                start = i + 1;
            }
        }

        if (start < path.Length)
        {
            list.Add(path.Slice(start));
        }

        RelativeName[] array = new RelativeName[list.Count];
        for (i = 0; i < list.Count; i++)
        {
            ReadOnlySpan<char> current = list[i];
            if (!RelativeName.TryParseOne(current, out RelativeName name))
            {
                throw new ArgumentException($"Invalid distinguished name component: {current.ToString()}", nameof(path));
            }

            array[i] = name;
        }

        list.Dispose();
        return array;
    }

    public static bool TrySplit(ReadOnlySpan<char> path, Span<RelativeName> destination, out int namesWritten)
    {
        if (destination.IsEmpty)
        {
            namesWritten = 0;
            return false;
        }

        SpanCharArray list = new(path.Length, COMMA);

        try
        {
            int start = 0;
            int i = 0;
            for (i = 0; i < path.Length; i++)
            {
                if (COMMA == path[i] && !path.IsEscapedAt(in i))
                {
                    list.Add(path.Slice(start, i - start));
                    start = i + 1;
                }
            }

            if (destination.Length < list.Count)
            {
                namesWritten = 0;
                return false;
            }

            for (i = 0; i < list.Count; i++)
            {
                ReadOnlySpan<char> current = list[i];
                if (!RelativeName.TryParseOne(current, out RelativeName name))
                {
                    namesWritten = 0;
                    return false;
                }

                destination[i] = name;
            }

            namesWritten = list.Count;
            return true;
        }
        finally
        {
            list.Dispose();
        }
    }
}

