using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AD.Api.Ldap.Path
{
    public sealed record PathValue
    {
        private readonly char[][] _charArrays = new char[7][];

        private const char FORWARD_SLASH = (char)47;
        private const char COLON = (char)58;
        private const char COMMA = (char)44;
        private StringBuilder? _builder;
        private readonly bool _useSsl;

        [NotNull]
        public string? Host
        {
            get => new(_charArrays[2]);
            init => _charArrays[2] = ValidateString(value);
        }

        [NotNull]
        public string? DistinguishedName
        {
            get => new(_charArrays[6]);
            init => _charArrays[6] = ValidateString(value);
        }
        private char[] DNArray
        {
            get => _charArrays[6] ?? Array.Empty<char>();
            set => _charArrays[6] = value is not null
                ? value
                : Array.Empty<char>();
        }
        public Protocol Protocol { get; }
        public bool UseSsl
        {
            get => _useSsl;
            init
            {
                _useSsl = value;
                switch (this.Protocol)
                {
                    case Protocol.Ldap:
                        _charArrays[4] = new char[3] { (char)54, (char)51, (char)54 }; // 636
                        break;

                    case Protocol.GlobalCatalog:
                        _charArrays[4][3] = (char)57;   // 3269
                        break;

                    default:
                        goto case Protocol.Ldap;
                }
            }
        }

        public PathValue(Protocol protocol = Protocol.Ldap)
        {
            this.Protocol = protocol;
            _charArrays[0] = new char[4] { (char)76, (char)68, (char)65, (char)80 }; // LDAP
            switch (protocol)
            {
                case Protocol.Ldap:
                    _charArrays[4] = new char[3] { (char)51, (char)56, (char)57 }; // 389
                    break;

                case Protocol.GlobalCatalog:
                    _charArrays[4] = new char[4] { (char)51, (char)50, (char)54, (char)56 }; // 3268
                    break;

                default:
                    goto case Protocol.Ldap;
            }
            
            _charArrays[1] = new char[3] { COLON, FORWARD_SLASH, FORWARD_SLASH };
            if (_charArrays[2] is null)
            {
                _charArrays[2] = Array.Empty<char>();
            }

            _charArrays[3] = new char[1] { COLON };
            _charArrays[5] = new char[1] { FORWARD_SLASH };
            _charArrays[6] = Array.Empty<char>();
        }
        
        internal void Clear()
        {
            Array.Clear(_charArrays);
            _builder?.Clear();
        }

        /// <summary>
        /// Generates a new <see cref="PathValue"/> of the parent path above the current record.
        /// </summary>
        /// <returns>
        ///     A new <see cref="PathValue"/> record is constructed going 1 level up in the path to the next node.
        ///     
        ///     However, if the current <see cref="PathValue"/> is at the top-most path or <see cref="DistinguishedName"/> is
        ///     <see cref="string.Empty"/>, then the current record is returned unaltered.
        /// </returns>
        public PathValue GetParent()
        {
            if (this.DNArray.Length <= 0 || (this.DNArray.Length >= 2 && this.DNArray[0] == (char)68 && this.DNArray[1] == (char)67))
                return this;

            Match parentMatch = Regex.Match(this.DistinguishedName, @"\,(?=(?:CN|OU|DC)\=)(.+)", RegexOptions.Compiled);
            if (parentMatch.Success && parentMatch.Groups.Count > 1 && parentMatch.Groups[1].Success)
            {
                PathValue pv = new(this.Protocol)
                {
                    Host = this.Host,
                    UseSsl = this.UseSsl
                };

                pv.DNArray = parentMatch.Groups[1].ValueSpan.ToArray();

                return pv;
            }

            return this;
        }

        /// <summary>
        /// Constructs a new <see cref="PathValue"/> by appending the specifying path onto the existing
        /// <see cref="DistinguishedName"/>.
        /// </summary>
        /// <param name="subPath">The sub path to build the new path from.</param>
        /// <returns>A new <see cref="PathValue"/> pointing to the specified sub path.  If <paramref name="subPath"/>
        /// is <see langword="null"/> or empty, then the current record is returned unaltered.</returns>
        public PathValue BuildSubLevel(string? subPath)
        {
            if (string.IsNullOrWhiteSpace(subPath))
                return this;

            PathValue subPv = new(this.Protocol)
            {
                Host = this.Host,
                UseSsl = this.UseSsl
            };

            subPv._charArrays[6] = new char[this.DNArray.Length];
            this.DNArray.CopyTo(subPv._charArrays[6], 0);

            ReadOnlySpan<char> subChars = subPath.AsSpan();

            char[] newArray = CreateNewArray(subPv.DNArray, subChars, out bool needsComma);

            if (TryPopulateArray(subPv.DNArray, subChars, needsComma, ref newArray))
            {
                subPv.DNArray = newArray;
            }

            return subPv;
        }

        /// <summary>
        /// Returns the full LDAP path.
        /// </summary>
        /// <returns>A <see cref="string"/> value of the LDAP path constructed.</returns>
        public string GetValue()
        {
            if (this.NeedToBuild())
            {
                for (int i = 0; i < _charArrays.Length; i++)
                {
                    char[] thisArray = _charArrays[i];
                    switch (i)
                    {
                        case 2:
                            if (thisArray is null || thisArray.Length <= 0)
                                i += 3;

                            else
                                goto default;
                            continue;

                        case 5:
                            if (this.DNArray is null || this.DNArray.Length <= 0)
                            {
                                i++;
                                break;
                            }

                            else
                                goto default;

                        default:
                            _builder.Append(thisArray);
                            break;
                    }
                }
            }

            return _builder.ToString();
        }

        /// <summary>
        /// Attempts to construct a new <see cref="PathValue"/> by appending the specifying path onto the existing 
        /// <see cref="DistinguishedName"/>.
        /// </summary>
        /// <param name="subPath">The sub path to build the new <see cref="PathValue"/> from.</param>
        /// <param name="subPathValue">The resulting path.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="subPathValue"/> was constructed and is not equal to
        ///     the current <see cref="PathValue"/> record; otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryBuildSubLevel([NotNullWhen(true)] string? subPath, out PathValue subPathValue)
        {
            subPathValue = this.BuildSubLevel(subPath);

            return !this.Equals(subPathValue);
        }

        /// <summary>
        /// Attempts to return the parent <see cref="PathValue"/> of the current record.
        /// </summary>
        /// <param name="parentValue">The parent <see cref="PathValue"/> or the current record.</param>
        /// <returns>
        ///     <see langword="true"/> if the parent was constructed successfully and is not equal to the current record.
        /// </returns>
        public bool TryGetParent(out PathValue parentValue)
        {
            parentValue = this.GetParent();

            return !this.Equals(parentValue);
        }

        public static implicit operator string(PathValue pathValue)
        {
            return pathValue.GetValue();
        }

        private static char[] CreateNewArray(char[] originalDNArray, ReadOnlySpan<char> subCharacters, out bool needsComma)
        {
            int originalLength = GetOriginalDNLength(originalDNArray, out needsComma);

            int newLength = subCharacters.Length + originalLength;
            char[] newArr = new char[newLength];

            if (needsComma)
                newArr[subCharacters.Length] = COMMA;

            return newArr;
        }

        private static int GetOriginalDNLength(char[] dnChars, out bool needsComma)
        {
            needsComma = false;
            int originalLength = dnChars.Length;
            if (originalLength > 0)
            {
                originalLength++;
                needsComma = true;
            }

            return originalLength;
        }

        [MemberNotNull(nameof(_builder))]
        private bool NeedToBuild()
        {
            if (_builder is null)
            {
                int total = _charArrays.Sum(x => x.Length);
                _builder = new StringBuilder(total);
                return true;
            }

            return false;
        }

        private static bool TryPopulateArray(char[] originalArray, ReadOnlySpan<char> subCharacters, bool needsComma, ref char[] newArray)
        {
            int destIndex = needsComma
                ? subCharacters.Length + 1
                : subCharacters.Length;

            Array.Copy(originalArray, 0, newArray, destIndex, originalArray.Length);

            return subCharacters.TryCopyTo(newArray);
        }

        private static char[] ValidateString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<char>();

            Span<char> span = new(value.ToCharArray());

            return span
                .TrimStart(FORWARD_SLASH).TrimStart(COLON).TrimStart(COMMA)
                .TrimEnd(FORWARD_SLASH).TrimEnd(COLON).TrimEnd(COMMA)
                .ToArray();
        }

        public static PathValue FromDirectoryEntry(DirectoryEntry directoryEntry)
        {
            Match regexMatch = Regex.Match(
                directoryEntry.Path,
                @"^(?'Protocol'(?:LDAP|GC))\:\/\/((?'Host'[a-zA-Z\.0-9]{1,})(?:\:|)(?'Port'\d{3,4}|)\/(?'DistinguishedName'.+)|(?'DistinguishedName'.+))$",
                RegexOptions.ExplicitCapture | RegexOptions.Compiled);

            if (!regexMatch.Success)
                throw new InvalidDataException(nameof(directoryEntry));

            Protocol protocol = Protocol.Ldap;
            if (regexMatch.Groups.TryGetValue(nameof(Protocol), out Group? protoGroup) && protoGroup.Success)
            {
                switch (protoGroup.Value)
                {
                    case "GC":
                        protocol = Protocol.GlobalCatalog;
                        break;

                    default:
                        break;
                }
            }

            string? host = null;
            if (regexMatch.Groups.TryGetValue(nameof(Host), out Group? hostGroup) && hostGroup.Success)
            {
                host = hostGroup.Value;
            }

            string? dn = null;
            if (regexMatch.Groups.TryGetValue(nameof(DistinguishedName), out Group? dnGroup) && dnGroup.Success)
            {
                dn = dnGroup.Value;
            }

            bool useSsl = false;
            if (regexMatch.Groups.TryGetValue("Port", out Group? portGroup) && portGroup.Success &&
                int.TryParse(portGroup.Value, out int port))
            {
                switch (port)
                {
                    case 636:
                        useSsl = true;
                        break;

                    case 3268:
                        protocol = Protocol.GlobalCatalog;
                        break;

                    case 3269:
                        useSsl = true;
                        goto case 3268;

                    default:
                        break;
                }
            }

            return new(protocol)
            {
                Host = host,
                DistinguishedName = dn,
                UseSsl = useSsl
            };
        }
    }
}
