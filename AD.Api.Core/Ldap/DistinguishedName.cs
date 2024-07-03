using AD.Api.Core.Ldap.Filters;
using AD.Api.Statics;
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

            int index = distinguishedName.IndexOf(',');
            if (index <= 0 || index >= distinguishedName.Length - 3)
            {
                return new(distinguishedName.ToString());
            }
            else if (distinguishedName[index - 1] == CharConstants.BACKSLASH)
            {
                ReadOnlySpan<char> working = distinguishedName.Slice(index + 1);
                while (index >= 0 && index < working.Length - 3)
                {
                    index = working.IndexOf(',');
                    if (index > 0 && working[index - 1] != CharConstants.BACKSLASH)
                    {
                        index = index + (distinguishedName.Length - working.Length);
                        break;
                    }
                    else if (index >= working.Length)
                    {
                        index = -1;
                        break;
                    }
                    else
                    {
                        working = working.Slice(index + 1);
                    }
                }

                if (index < 0)
                {
                    return new(distinguishedName.ToString());
                }
            }

            ReadOnlySpan<char> commonName = distinguishedName.Slice(0, index);
            ReadOnlySpan<char> parentPath = distinguishedName.Slice(index + 1);
            return new(commonName.ToString(), parentPath.ToString());
        }
    }
}
