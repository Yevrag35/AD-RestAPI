using AD.Api.Core.Ldap.Filters;
using AD.Api.Spans;
using AD.Api.Statics;
using System.Buffers;
using System.Runtime.InteropServices;

namespace AD.Api.Core.Ldap
{
    [StructLayout(LayoutKind.Auto)]
    public sealed class DistinguishedName : IEquatable<DistinguishedName>
    {
        private readonly record struct Constructing(string Name, string Path, bool NeedsComma, bool NeedsPrefix);

        public const string CommonNamePrefix = "CN=";
        public const string OrganizationalUnitPrefix = "OU=";
        private const int MAX_LENGTH = 400;

        private string? _fullValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _commonName;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _parentPath;

        //private static readonly char[] _dnChars = ;
        public static readonly SearchValues<char> EscapedChars = SearchValues
            .Create([',', '\\', '=', '+', '>', '<', ';', '"']);

        [MemberNotNullWhen(true, nameof(_fullValue))]
        private bool IsConstructed { get; set; }

        public string CommonName
        {
            get => _commonName;
            set
            {
                SetFieldValue(value, ref _commonName);
                this.ResetValue();
            }
        }
        public string Path
        {
            get => _parentPath;
            set
            {
                SetFieldValue(value, ref _parentPath);
                this.ResetValue();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DistinguishedName()
        {
            _commonName = string.Empty;
            _parentPath = string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonName"></param>
        /// <param name="parentPath"></param>
        /// <inheritdoc cref="ArgumentException.ThrowIfNullOrWhiteSpace(string?, string?)" path="/exception"/>
        public DistinguishedName(string commonName, string? parentPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(commonName);
            SetFieldValue(commonName, ref _commonName);
            SetFieldValue(parentPath, ref _parentPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <inheritdoc cref="ArgumentException.ThrowIfNullOrWhiteSpace(string?, string?)" path="/exception"/>
        public DistinguishedName(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            SetFieldValue(_commonName, ref _commonName);
            _parentPath = string.Empty;
        }

        public bool Equals(DistinguishedName? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            else if (other is null)
            {
                return false;
            }
            else
            {
                return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
        public override bool Equals(object? obj)
        {
            if (obj is DistinguishedName dn)
            {
                return this.Equals(dn);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.ToString());
        }
        public static bool HasWellKnownPath(FilteredRequestType requestType)
        {
            switch (requestType)
            {
                case FilteredRequestType.Any:
                case FilteredRequestType.Container:
                case FilteredRequestType.OrganizationalUnit:
                    goto default;

                case FilteredRequestType.User:
                case FilteredRequestType.Computer:
                case FilteredRequestType.Group:
                case FilteredRequestType.Contact:
                case FilteredRequestType.ManagedServiceAccount:
                    return true;

                default:
                    return false;
            }
        }
        /// <summary>
        /// Returns the string representation of the full distinguishedName.
        /// </summary>
        public override string ToString()
        {
            if (!this.IsConstructed)
            {
                _fullValue = this.Construct();
            }

            return _fullValue;
        }

        private string Construct()
        {
            scoped ReadOnlySpan<char> name = _commonName;
            if (name.IsWhiteSpace())
            {
                return _parentPath;
            }

            int extras = !string.IsNullOrWhiteSpace(_parentPath) ? 1 : 0;
            bool needsComma = extras == 1;
            bool needsPrefix = !name.StartsWith(CommonNamePrefix, StringComparison.OrdinalIgnoreCase);
            if (needsPrefix)
            {
                extras += CommonNamePrefix.Length;
            }

            int length = _commonName.Length + _parentPath.Length + extras;  // 4 is for 1 comma and the possible "CN="

            Constructing state = new(_commonName, _parentPath, needsComma, needsPrefix);
            return string.Create(length, state, ConstructingFullValue);
        }
        private static void ConstructingFullValue(Span<char> buffer, Constructing state)
        {
            int pos = 0;
            if (state.NeedsPrefix)
            {
                CommonNamePrefix.CopyToSlice(buffer, ref pos);
            }

            state.Name.CopyTo(buffer.Slice(0, pos));
            if (state.NeedsComma)
            {
                pos += state.Name.Length;
                buffer[pos++] = CharConstants.COMMA;

                state.Path.CopyTo(buffer.Slice(0, pos));
            }
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

            ReadOnlySpan<char> checkSlice = working.Slice(index - 2, 3);

            return checkSlice.Equals(CommonNamePrefix, StringComparison.OrdinalIgnoreCase)
                || checkSlice.Equals(OrganizationalUnitPrefix, StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsProperComma(ReadOnlySpan<char> working, in int index)
        {
            if (index + 3 >= working.Length)
            {
                return false;
            }

            ReadOnlySpan<char> checkSlice = working.Slice(index + 1, 3);
            return checkSlice.Equals(CommonNamePrefix, StringComparison.OrdinalIgnoreCase)
                || checkSlice.Equals(OrganizationalUnitPrefix, StringComparison.OrdinalIgnoreCase);
        }
        private void ResetValue()
        {
            _fullValue = null;
            this.IsConstructed = false;
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

