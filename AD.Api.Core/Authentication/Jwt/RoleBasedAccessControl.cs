using AD.Api.Components;
using BCrypt.Net;

namespace AD.Api.Core.Authentication.Jwt
{
    public sealed class JsonRoleBasedAccessControl
    {
        public AuthorizationScope[] Scopes { get; init; } = [];
        public AuthorizedUser[] Users { get; init; } = [];
    }

    public sealed class AuthorizationScope
    {
        public const string CLAIM_TYPE = "scopes";
        public static readonly StaticKey NeedsScoping = StaticKey.Create(CLAIM_TYPE);
        public const string ANY = "*";

        private bool _isBaseScoped;
        private bool _isDomainScoped;
        private string _domain = string.Empty;
        private string _base = string.Empty;

        public string Base
        {
            get => _base;
            set => _base = GuardValue(value, ref _isBaseScoped);
        }
        public required string Key { get; init; }
        public string Domain
        {
            get => _domain ??= string.Empty;
            set => _domain = GuardValue(value, ref _isDomainScoped);
        }
        public bool IsBaseScoped => _isBaseScoped;
        public bool IsDomainScoped => _isDomainScoped;
        public required AuthorizedRole Roles { get; init; }

        public bool IsAuthorized(in WorkingScope scope)
        {
            if (!this.Roles.HasFlag(scope.RequiredRole))
            {
                return false;
            }

            if (this.IsDomainScoped && !this.Domain.Equals(scope.DomainName))
            {
                return false;
            }

            return !this.IsBaseScoped || IsInBaseScope(this.Base, scope.DistinguishedName);
        }

        private static string GuardValue(string? value, ref bool field)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                field = false;
                return string.Empty;
            }
            else
            {
                field = true;
                return value;
            }
        }
        private static bool IsInBaseScope(ReadOnlySpan<char> baseScope, ReadOnlySpan<char> distinguishedName)
        {
            return distinguishedName.EndsWith(baseScope, StringComparison.OrdinalIgnoreCase);
        }
    }
    public sealed class AuthorizedUser
    {
        public required string UserName { get; init; }
        public string UserDisplayName { get; set; } = string.Empty;
        public required string UserHash { get; init; }
        public required HashType HashType { get; init; }
        public required AuthorizedRole Roles { get; init; }
        public string[] Scopes { get; init; } = [];
    }
}

