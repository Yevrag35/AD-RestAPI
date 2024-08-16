using AD.Api.Statics;

namespace AD.Api.Core.Ldap;

public readonly partial struct DistinguishedName
{
    private static int CopyTo(ReadOnlySpan<RelativeName> segments, Span<char> destination)
    {
        ref readonly RelativeName first = ref segments[0];
        first.Value.CopyTo(destination);
        int written = first.Value.Length;

        foreach (RelativeName relativeName in segments.Slice(1))
        {
            destination[written++] = CharConstants.COMMA;
            relativeName.Value.CopyToSlice(destination, ref written);
        }

        return written;
    }

    private static int GetTotalLength(ReadOnlySpan<RelativeName> relativeNames)
    {
        if (relativeNames.IsEmpty)
        {
            return 0;
        }

        int length = relativeNames.Length - 1;
        foreach (RelativeName relativeName in relativeNames)
        {
            length += relativeName.IsEmpty ? 0 : relativeName.Value.Length;
        }

        return length;
    }
    private static string ToString(ReadOnlySpan<RelativeName> segments, in int totalLength)
    {
        Span<char> chars = stackalloc char[totalLength];
        int written = CopyTo(segments, chars);

        return new string(chars.Slice(0, written));
    }
}