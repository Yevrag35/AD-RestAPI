using AD.Api.Collections.Enumerators;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;

namespace AD.Api.Core.Ldap
{
    public readonly partial struct RelativeName
    {
        //private static ReadOnlySpan<char> EscapeChars(ReadOnlySpan<char> source, Span<char> destination)
        //{
        //    if (!source.ContainsAny(NonStandardEscapedChars) && CharConstants.POUND != source[0])
        //    {
        //        return source;
        //    }

        //    int position = 0;

        //    if (CharConstants.POUND == source[0])
        //    {
        //        destination[position++] = '\\';
        //        destination[position++] = CharConstants.POUND;
        //    }

        //    destination = EscapeCharacters(source, destination, ref position);
        //    if (destination.IsEmpty)
        //    {
        //        return []; // Will be treated as invalid;
        //    }

        //    destination = EscapeSpaces(destination, ref position);

        //    return destination.Slice(0, position);
        //}
        //private static Span<char> EscapeSpaces(Span<char> buffer, scoped ref int position)
        //{
        //    Span<char> working = buffer.Slice(0, position);
        //    int nonSpaceIndex = working.LastIndexOfAnyExcept(CharConstants.SPACE);
        //    if (nonSpaceIndex != working.Length - 1)
        //    {
        //        int p = 0;
        //        Span<char> spaces = buffer.Slice(nonSpaceIndex + 1);
        //        foreach (char sp in spaces)
        //        {
        //            buffer[p++] = CharConstants.BACKSLASH;
        //            buffer[p++] = sp;
        //        }

        //        position += spaces.Length;
        //    }

        //    return buffer;
        //}
        private static bool IsProperEquals(ReadOnlySpan<char> working, in int index)
        {
            if (index < 1 || index >= working.Length - 1)
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

        public static bool IsValid(ReadOnlySpan<char> value)
        {
            if (value.IsWhiteSpace())
            {
                return false;
            }

            if (CharConstants.POUND == value[0])
            {
                return false;
            }
            else if (value.Length > 2 && CharConstants.SPACE == value[^1] && !value.IsEscapedAt(value.Length - 1))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                ref readonly char c = ref value[i];
                if (NonStandardEscapedChars.Contains(c) && !value.IsEscapedAt(in i))
                {
                    return false;
                }

                switch (c)
                {
                    case CharConstants.EQUALS:
                        if (!IsProperEquals(value, in i))
                        {
                            return false;
                        }

                        break;

                    case CharConstants.COMMA:
                        if (!value.IsEscapedAt(in i))
                        {
                            return false;
                        }

                        break;

                    case CharConstants.BACKSLASH:
                        if (!IsProperBackslash(value, i))
                        {
                            return false;
                        }

                        break;

                    default:
                        break;
                }
            }

            return true;
        }

        private static bool IsProperBackslash(ReadOnlySpan<char> value, int index)
        {
            if (index == value.Length - 1)
            {
                return value.IsEscapedAt(in index);
            }

            ref readonly char nextChar = ref value[index + 1];

            return AllEscapedChars.Contains(nextChar) || value.IsEscapedAt(in index);
        }
    }
}
