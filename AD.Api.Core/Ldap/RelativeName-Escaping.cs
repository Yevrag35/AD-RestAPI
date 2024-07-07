using AD.Api.Collections.Enumerators;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;

namespace AD.Api.Core.Ldap
{
    public readonly partial struct RelativeName
    {
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
            if (destination.IsEmpty)
            {
                return []; // Will be treated as invalid;
            }

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
                        if (!IsProperEquals(working, in i))
                        {
                            return []; // Will be treated as invalid;
                        }

                        buffer[position++] = c;
                        break;

                    case CharConstants.COMMA:
                        if (working.IsEscapedAt(in i))
                        {
                            buffer[position++] = c;
                            break;
                        }

                        goto default;

                    case CharConstants.BACKSLASH:
                        if (working.IsEscapedAt(i + 1))
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
            if (index < 2 || index >= working.Length - 1)
            {
                return false;
            }

            return IsValidPrefixNoError(working.Slice(0, index));
        }
        private static bool IsValidPrefixNoError(ReadOnlySpan<char> working)
        {
            ArrayRefEnumerator<string> enumerator = new(_attributeValues.Keys.AsSpan());
            bool flag = false;
            while (enumerator.MoveNext(in flag))
            {
                flag = working.Equals(enumerator.Current.AsSpan(0, enumerator.Current.Length - 1), StringComparison.OrdinalIgnoreCase);
            }

            return flag;
        }
    }
}
