using AD.Api.Components;
using AD.Api.Statics;
using AD.Api.Strings;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    internal sealed class LdapIdentifierDictionary
    {
        private readonly record struct DcNameTuple([Required] string DomainController, [Required] string DomainName) :
            IStringCreatable<DcNameTuple>
        {
            public readonly int Length => this.DomainController.Length + this.DomainName.Length + 1;

            public readonly void WriteTo(scoped Span<char> chars)
            {
                this.DomainController.CopyTo(chars);
                int pos = this.DomainController.Length;
                chars[pos++] = CharConstants.PERIOD;
                this.DomainName.CopyTo(chars.Slice(pos));
            }
        }

        private readonly ConcurrentDictionary<string, LdapDirectoryIdentifier> _dict;
        private readonly string _domainName;

        public LdapDirectoryIdentifier Default => _dict[string.Empty];

        public int Count => _dict.Count;
        public string DomainName => _domainName;

        internal LdapIdentifierDictionary(string domainName, LdapDirectoryIdentifier defaultIdentifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(domainName);
            _domainName = domainName;
            var defaultEntries = CreateDefaultEntries(domainName, defaultIdentifier);

            _dict = new(Environment.ProcessorCount, defaultEntries, StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<KeyValuePair<string, LdapDirectoryIdentifier>> CreateDefaultEntries(string domainName, LdapDirectoryIdentifier defaultIdentifier)
        {
            yield return new(string.Empty, defaultIdentifier);
            yield return new(domainName, defaultIdentifier);
        }

        private string FormatDomainController(string domainController)
        {
            if (domainController.EndsWith(this.DomainName, StringComparison.OrdinalIgnoreCase))
            {
                return domainController;
            }

            return new DcNameTuple(domainController, this.DomainName).CreateString();
        }
        public LdapDirectoryIdentifier GetOrAdd(string? domainController)
        {
            if (string.IsNullOrWhiteSpace(domainController))
            {
                return this.Default;
            }
            
            if (_dict.TryGetValue(domainController, out LdapDirectoryIdentifier? identifier))
            {
                return identifier;
            }

            string key = domainController;
            domainController = this.FormatDomainController(domainController);

            identifier = new(domainController, _dict[string.Empty].PortNumber, false, false);
            _ = _dict.TryAdd(domainController, identifier);
            if (!key.Equals(domainController))
            {
                _ = _dict.TryAdd(key, identifier);
            }

            return identifier;
        }

        public bool TryGetValue(string? domainKey, [NotNullWhen(true)] out LdapDirectoryIdentifier? identifier)
        {
            return _dict.TryGetValue(domainKey ?? string.Empty, out identifier);
        }

        public bool TryRemove([MaybeNullWhen(false)] string key)
        {
            if (string.IsNullOrWhiteSpace(key) || this.DomainName.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (_dict.TryRemove(key, out _))
            {
                if (!key.EndsWith(this.DomainName, StringComparison.OrdinalIgnoreCase))
                {
                    key = this.FormatDomainController(key);
                    return _dict.TryRemove(key, out _);
                }

                return true;
            }

            return false;
        }
    }
}

