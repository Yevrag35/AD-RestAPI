using AD.Api.Core.Ldap.Filters;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using System.Buffers;

namespace AD.Api.Core.Ldap
{
    public sealed partial class DistinguishedName : IEquatable<DistinguishedName>
    {
        public const string DomainComponentPrefix = "DC=";
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
        
        private void ResetValue()
        {
            _fullValue = null;
            this.IsConstructed = false;
        }
        /// <summary>
        /// Returns the string representation of the full distinguishedName.
        /// </summary>
        public override string ToString()
        {
            if (!this.IsConstructed)
            {
                _fullValue = this.Construct();
                this.IsConstructed = true;
            }

            return _fullValue;
        }

        public static DistinguishedName Parse(ReadOnlySpan<char> distinguishedName)
        {
            if (distinguishedName.IsWhiteSpace())
            {
                return new();
            }

            int index = 0;
            while (index < distinguishedName.Length)
            {
                int commaIndex = distinguishedName.Slice(index).IndexOf(',');
                if (commaIndex < 0)
                {
                    // No commas found at all, return the full DN as common name.
                    return new(distinguishedName.ToString());
                }

                // Adjust index relative to the original span.
                index += commaIndex;

                // Check if the comma is escaped
                if (!distinguishedName.IsEscapedAt(in index))
                {
                    // Found an unescaped comma.
                    break;
                }

                index++;
            }

            if (index < 0 || index >= distinguishedName.Length - 3)
            {
                // No valid comma found or comma is at an invalid position.
                Debug.Fail("What is this?");
                return new(distinguishedName.ToString());
            }

            ReadOnlySpan<char> commonName = distinguishedName.Slice(0, index);
            ReadOnlySpan<char> parentPath = distinguishedName.Slice(index + 1);
            return new(commonName.ToString(), parentPath.ToString());
        }

        //private static bool IsEscapedAt(in int index, ReadOnlySpan<char> value)
        //{
        //    int backslashCount = 0;
        //    // Count the number of backslashes preceding the index.
        //    for (int i = index - 1; i >= 0 && CharConstants.BACKSLASH == value[i]; i--)
        //    {
        //        backslashCount++;
        //    }

        //    return 0 != backslashCount % 2;
        //}
    }
}
