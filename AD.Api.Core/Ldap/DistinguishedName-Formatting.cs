using AD.Api.Spans;
using AD.Api.Statics;

namespace AD.Api.Core.Ldap
{
    public sealed partial class DistinguishedName
    {
        private readonly record struct Constructing(string Name, string Path, bool NeedsComma, bool NeedsPrefix);

        private static void ConstructingFullValue(Span<char> buffer, Constructing state)
        {
            int pos = 0;
            if (state.NeedsPrefix)
            {
                CommonNamePrefix.CopyToSlice(buffer, ref pos);
            }

            state.Name.CopyTo(buffer.Slice(pos));
            if (state.NeedsComma)
            {
                pos += state.Name.Length;
                buffer[pos++] = CharConstants.COMMA;

                state.Path.CopyTo(buffer.Slice(pos));
            }
        }
        private static bool EqualsAnyPrefix(ReadOnlySpan<char> slice)
        {
            return slice.Equals(CommonNamePrefix, StringComparison.OrdinalIgnoreCase)
                || slice.Equals(OrganizationalUnitPrefix, StringComparison.OrdinalIgnoreCase)
                || slice.Equals(DomainComponentPrefix, StringComparison.OrdinalIgnoreCase);
        }
        private static ReadOnlySpan<char> EscapeChars(ReadOnlySpan<char> source, Span<char> destination)
        {
            if (!source.ContainsAny(EscapedChars) && CharConstants.POUND != source[0])
            {
                return source;
            }

            int position = 0;

            if (CharConstants.POUND == source[0])
            {
                destination[position++] = '\\';
                destination[position++] = CharConstants.POUND;
            }

            destination = EscapeCharacters(source, destination, ref position);
            destination = EscapeSpaces(destination, ref position);

            return destination.Slice(0, position);
        }
        private static Span<char> EscapeCharacters(ReadOnlySpan<char> source, Span<char> buffer, scoped ref int position)
        {
            ReadOnlySpan<char> working = source.Slice(position);
            for (int i = 0; i < working.Length; i++)
            {
                char c = working[i];
                if (!EscapedChars.Contains(c))
                {
                    buffer[position++] = c;
                    continue;
                }

                switch (c)
                {
                    case CharConstants.EQUALS:
                        if (IsProperEquals(working, in i))
                        {
                            buffer[position++] = c;
                            break;
                        }

                        goto default;

                    case CharConstants.COMMA:
                        if (IsProperComma(working, in i))
                        {
                            buffer[position++] = c;
                            break;
                        }

                        goto default;

                    default:
                        buffer[position++] = CharConstants.BACKSLASH;
                        buffer[position++] = c;
                        break;
                }
            }

            return buffer;
        }
        private static Span<char> EscapeSpaces(Span<char> buffer, scoped ref int position)
        {
            Span<char> working = buffer.Slice(0, position);
            int nonSpaceIndex = working.LastIndexOfAnyExcept(CharConstants.SPACE);
            if (nonSpaceIndex != working.Length - 1)
            {
                int p = 0;
                Span<char> spaces = buffer.Slice(nonSpaceIndex + 1);
                foreach (char sp in spaces)
                {
                    buffer[p++] = CharConstants.BACKSLASH;
                    buffer[p++] = sp;
                }

                position += spaces.Length;
            }

            return buffer;
        }
        private static bool IsProperEquals(ReadOnlySpan<char> working, in int index)
        {
            if (index < 2)
            {
                return false;
            }

            return EqualsAnyPrefix(working.Slice(index - 2, 3));
        }
        private static bool IsProperComma(ReadOnlySpan<char> working, in int index)
        {
            if (index + 3 >= working.Length)
            {
                return false;
            }

            return EqualsAnyPrefix(working.Slice(index + 1, 3));
        }
        private static void SetFieldValue(string? newValue, [NotNull] ref string? field)
        {
            field ??= string.Empty;
            scoped ReadOnlySpan<char> newVal = newValue.AsSpan();

            if (newVal.IsWhiteSpace() || newVal.Equals(field, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            newVal = EscapeChars(newVal, stackalloc char[newVal.Length * 2]);
            field = newVal.ToString();
        }
    }
}

