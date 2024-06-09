using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace AD.Api.Ldap.Properties
{
    public sealed class ProxyAddress : IEquatable<ProxyAddress>
    {
        private bool _isPrimary;
        private const char AT_SIGN = (char)64;
        private const char COLON = (char)58;
        private static readonly char[] PRIMARY = new char[5] { (char)83, (char)77, (char)84, (char)80, COLON };
        private static readonly char[] SECONDARY = new char[5] { (char)115, (char)109, (char)116, (char)112, COLON };

        private readonly char[][] _charArrays = new char[4][];
        private char[]? _raw;
        private readonly Lazy<StringBuilder> _builder = new();

        public string Domain => new(_charArrays[3]);
        public bool IsPrimary
        {
            get => _isPrimary && this.IsValid;
            set
            {
                if (!this.IsValid)
                    return; // don't bother...

                _isPrimary = value;
                switch (value)
                {
                    case true:
                        _charArrays[0] = PRIMARY;
                        break;

                    case false:
                        _charArrays[0] = SECONDARY;
                        break;
                }
            }
        }
        private bool IsSizeEnsured => _builder.IsValueCreated && _builder.Value.Capacity > 0;
        [MemberNotNullWhen(false, "_raw")]
        public bool IsValid => _raw is null;
        public string Prefix => new(_charArrays[1]);

        public ProxyAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException(nameof(address));

            if (!this.ParseAddress(address))
            {
                _raw = address.ToCharArray();
                Array.Clear(_charArrays);
            }
            else
            {
                _charArrays[2] = new char[1] { AT_SIGN };
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProxyAddress pa)
                return this.Equals(pa);

            else if (obj is string paStr)
                return StringComparer.CurrentCultureIgnoreCase.Equals(this.GetValue(), paStr);

            else
                return false;
        }
        public bool Equals(ProxyAddress? other)
        {
            if (other is null)
                return false;

            else if (ReferenceEquals(this, other))
                return true;

            if (this.IsValid && other.IsValid)
            {
                for (int i = 0; i < _charArrays.Length; i++)
                {
                    if (!ValidateArray(_charArrays[i], other._charArrays[i], i == 0))
                        return false;
                }

                return true;
            }

            return !this.IsValid && !other.IsValid
                && ValidateArray(_raw, other._raw, true);
        }
        public override int GetHashCode()
        {
            return StringComparer.CurrentCultureIgnoreCase.GetHashCode(this.GetValue());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public string GetValue()
        {
            if (!this.IsValid)
                return new string(_raw);

            return this
                .EnsureStringBuilder()
                .ToString();
        }

        private StringBuilder EnsureStringBuilder()
        {
            if (!this.IsValid)
                return _builder.Value;

            if (!this.IsSizeEnsured)
                _ = _builder.Value.EnsureCapacity(_charArrays.Sum(x => x.Length));

            else
                _ = _builder.Value.Clear();

            Array.ForEach(_charArrays, arr =>
            {
                _ = _builder.Value.Append(arr);
            });

            return _builder.Value;
        }

        private static Match MatchAddressSegments(string address)
        {
            return Regex.Match(
                address,
                @"^(?'IsPrimary'smtp\:|\:|)(?'Prefix'.+)\@(?'Domain'.*)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
        }

        private bool ParseAddress(string address)
        {
            Match match = MatchAddressSegments(address);
            if (!match.Success)
                throw new InvalidOperationException($"'{address}' is an invalid entry.");

            if (!match.Groups.TryGetValue(nameof(IsPrimary), out Group? group) || !group.Success)
                return false;

            this.SetWhetherPrimary(group.ValueSpan);

            if (!match.Groups.TryGetValue(nameof(this.Prefix), out Group? prefixGroup) || !prefixGroup.Success)
                return false;

            this.SetPrefix(prefixGroup.ValueSpan);

            if (!match.Groups.TryGetValue(nameof(this.Domain), out Group? domainGroup) || !domainGroup.Success)
                return false;

            this.SetDomain(domainGroup.ValueSpan);
            return true;
        }

        private void SetDomain(ReadOnlySpan<char> span)
        {
            if (!(_charArrays[3] is null))
            {
                Array.Clear(_charArrays[3]);
                Array.Resize(ref _charArrays[3], span.Length);
            }
            else
            {
                _charArrays[3] = new char[span.Length];
            }

            span.CopyTo(_charArrays[3]);
        }
        private void SetPrefix(ReadOnlySpan<char> span)
        {
            if (!(_charArrays[1] is null))
            {
                Array.Clear(_charArrays[1]);
                Array.Resize(ref _charArrays[1], span.Length);
            }
            else
            {
                _charArrays[1] = new char[span.Length];
            }

            span.CopyTo(_charArrays[1]);
        }
        private void SetWhetherPrimary(ReadOnlySpan<char> span)
        {
            if (span.SequenceEqual(PRIMARY))
            {
                this.IsPrimary = true;
                _charArrays[0] = PRIMARY;
            }
            else
            {
                _charArrays[0] = SECONDARY;
            }
        }

        private static bool ValidateArray(char[] x, char[] y, bool isSmtpArray)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (!isSmtpArray && char.ToUpper(x[i]) != char.ToUpper(y[i]))  // ignoring case.
                    return false;

                else if (isSmtpArray && x[i] != y[i])
                    return false;
            }

            return true;
        }

        public static implicit operator ProxyAddress(string rawAddress)
        {
            return new ProxyAddress(rawAddress);
        }
        public static implicit operator string(ProxyAddress proxyAddress)
        {
            return proxyAddress.GetValue();
        }
    }
}
