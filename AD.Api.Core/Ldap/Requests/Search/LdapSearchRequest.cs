using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Settings;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Requests.Search
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
    public sealed class LdapSearchRequest : LdapRequest
    {
        private static readonly string _defaultRequestId = Guid.Empty.ToString();
        private readonly ISearchDefaults _defaults;
        private bool _hasDefaults;
        private readonly SearchRequest _request;
        private Guid _requestId;
        protected override DirectoryRequest BackingRequest => _request;
        protected override string DefaultRequestId => _defaultRequestId;

        public string Filter
        {
            [DebuggerStepThrough]
            get => (string)_request.Filter;
            [DebuggerStepThrough]
            set => _request.Filter = value ?? string.Empty;
        }
        public Guid RequestId
        {
            [DebuggerStepThrough]
            get => _requestId;
            set
            {
                if (value == _requestId)
                {
                    return;
                }
                else if (value == Guid.Empty)
                {
                    _request.RequestId = _defaultRequestId;
                    _requestId = Guid.Empty;
                }
                else
                {
                    _requestId = value;
                    _request.RequestId = value.ToString();
                }
            }
        }
        public string SearchBase
        {
            [DebuggerStepThrough]
            get => _request.DistinguishedName;
            [DebuggerStepThrough]
            set => _request.DistinguishedName = value ?? string.Empty;
        }

        public LdapSearchRequest(ISearchDefaults searchDefaults)
        {
            _requestId = Guid.Empty;
            _defaults = searchDefaults;
            _request = new();
            ResetRequest(_request, searchDefaults);
            _hasDefaults = true;
        }

        public void AddAttributes(ReadOnlySpan<char> attributeString)
        {
            bool wantsDefault = false;
            char separator = attributeString.Contains(CharConstants.COMMA) ? CharConstants.COMMA : CharConstants.SPACE;
            Span<char> defaults = ['d', 'e', 'f', 'a', 'u', 'l', 't', 's'];

            foreach (ReadOnlySpan<char> section in attributeString.SpanSplit(in separator))
            {
                if (section.Equals(defaults.Slice(0, defaults.Length - 1), StringComparison.OrdinalIgnoreCase)
                    ||
                    section.Equals(defaults, StringComparison.OrdinalIgnoreCase))
                {
                    wantsDefault = true;
                }
                else if (!section.IsWhiteSpace())
                {
                    string s = section.ToString();
                    _request.Attributes.Add(s);
                }
            }

            if (!wantsDefault)
            {
                this.RemoveDefaultAttributes();
            }
        }
        public bool ApplyConnection([NotNull] ref string? domain, IConnectionService connectionService, [NotNullWhen(true)] out LdapConnection? connection)
        {
            domain ??= string.Empty;

            if (!connectionService.RegisteredConnections.TryGetValue(domain, out ConnectionContext? context))
            {
                connection = null;
                return false;
            }

            this.SearchBase = context.DefaultNamingContext;
            connection = context.CreateConnection();
            return true;
        }
        public void RemoveDefaultAttributes()
        {
            if (!_hasDefaults)
            {
                return;
            }

            for (int i = _defaults.Attributes.Length - 1; i >= 0; i--)
            {
                _request.Attributes.RemoveAt(i);
            }

            _hasDefaults = false;
        }
        protected override void ResetCore()
        {
            _requestId = Guid.Empty;
            ResetRequest(_request, _defaults);
            _hasDefaults = true;
        }
        private static void ResetRequest(SearchRequest request, ISearchDefaults defaults)
        {
            request.Aliases = defaults.DereferenceAlias;
            request.Attributes.Clear();
            request.Attributes.AddRange(defaults.Attributes);
            request.DistinguishedName = string.Empty;
            request.Filter = string.Empty;
            request.Scope = defaults.Scope;
            request.SizeLimit = defaults.SizeLimit;
            request.TimeLimit = defaults.Timeout;
        }

        public static implicit operator SearchRequest(LdapSearchRequest request)
        {
            return request._request;
        }
    }
}

