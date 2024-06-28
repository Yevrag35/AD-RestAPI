using AD.Api.Core.Ldap.Filters;
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
    }
}
