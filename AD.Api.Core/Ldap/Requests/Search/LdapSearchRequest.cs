using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Controls;
using AD.Api.Core.Settings;
using AD.Api.Statics;
using AD.Api.Strings.Extensions;
using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap
{
    [DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
    public sealed class LdapSearchRequest : LdapRequest, IResettable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly string _defaultRequestId = Guid.Empty.ToString();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDefaults _defaults;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly SearchRequest _request;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Guid _requestId;
        private readonly StatedDirectoryControl<PageResultRequestControl> _pageControl;
        private int _pageSize;

        private bool _hasDefaults;
        protected override DirectoryRequest BackingRequest => _request;
        protected override string DefaultRequestId => _defaultRequestId;

        /// <summary>
        /// The <see cref="LdapSearchRequest.Filter"/> contains the search filter for the LDAP request.
        /// </summary>
        /// <returns>
        /// The search filter for the LDAP request as a <see cref="string"/> value.
        /// </returns>
        /// <inheritdoc cref="SearchRequest.Filter" path="/exception"/>
        public string Filter
        {
            [DebuggerStepThrough]
            get => (string)_request.Filter;
            [DebuggerStepThrough]
            set => _request.Filter = value ?? string.Empty;
        }
        public int PageSize
        {
            get => _pageSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(this.PageSize));
                _pageSize = value;
                if (value == 0)
                {
                    _pageControl.AddToRequest = false;
                }
                else
                {
                    _pageControl.AddToRequest = true;
                    _pageControl.ChangeState(value, (size, control) => control.PageSize = size);
                }
            }
        }
        /// <summary>
        /// The <see cref="RequestId"/> contains the unique identifier for the LDAP request.
        /// </summary>
        /// <remarks>
        /// Each request will have its own RequestId per scoped-HTTP request.
        /// </remarks>
        /// <returns>
        /// The requestID for the LDAP request as a <see cref="Guid"/> value.
        /// </returns>
        public Guid RequestId
        {
            [DebuggerStepThrough]
            get => _requestId;
            set
            {
                if (value == Guid.Empty)
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
        /// <inheritdoc cref="SearchRequest.SizeLimit" path="/*[not(self::summary)]"/>
        /// <summary>
        ///     <inheritdoc cref="SearchRequest.SizeLimit" path="/summary/text()[1]"/>
        ///     <see cref="LdapSearchRequest"/>
        ///     <inheritdoc cref="SearchRequest.SizeLimit" path="/summary/text()[last()]"/>
        /// </summary>
        public int SizeLimit
        {
            [DebuggerStepThrough]
            get => _request.SizeLimit;
            [DebuggerStepThrough]
            set => _request.SizeLimit = value;
        }
        /// <summary>
        /// Gets or sets the base distinguished name (DN) from which the search will start.
        /// </summary>
        /// <remarks>
        /// The <see cref="SearchBase"/> property defines the starting point in the directory
        /// from which the LDAP search will be conducted. It is specified as a distinguished name (DN).
        /// </remarks>
        /// <returns>
        /// The base distinguished name (DN) for the LDAP search as a <see cref="string"/> value.
        /// </returns>
        public string SearchBase
        {
            [DebuggerStepThrough]
            get => _request.DistinguishedName;
            [DebuggerStepThrough]
            set => _request.DistinguishedName = value ?? string.Empty;
        }

        public LdapSearchRequest(IDefaults defaults)
        {
            _requestId = Guid.Empty;
            _defaults = defaults;
            _request = new();

            ref readonly ISearchDefaults globals = ref _defaults[string.Empty];

            _pageControl = new(new PageResultRequestControl(globals.SizeLimit), x => x.Cookie = []);
            ResetRequest(_request, _pageControl, in globals);
            _hasDefaults = true;
        }

        //public void CopyTo(LdapSearchRequest other)
        //{
        //    ArgumentNullException.ThrowIfNull(other);
        //    other._request.Attributes.Clear();
        //    for (int i = 0; i < _request.Attributes.Count; i++)
        //    {
        //        _ = other._request.Attributes.Add(_request.Attributes[i]);
        //    }

        //    other._pageSize = _pageSize;
        //    other.Filter = this.Filter;
        //    other.SizeLimit = this.SizeLimit;
        //    other.SearchBase = this.SearchBase;
        //    other._hasDefaults = _hasDefaults;
        //    other._requestId = _requestId;
        //    other._pageControl.AddToRequest = _pageControl.AddToRequest;
        //    other._pageControl.ChangeState(this, (s, c) =>
        //    {
        //        c.PageSize = s._pageSize;
        //        c.Cookie = s._pageControl.GetControlValue(x => x.Cookie);
        //    });
        //}

        public void AddAttributes(ReadOnlySpan<char> attributeString, FilteredRequestType? types)
        {
            if (attributeString.IsWhiteSpace())
            {
                this.AddAttributesFromTypes(types);
                return;
            }

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
            else
            {
                this.AddAttributesFromTypes(types);
            }
        }
        private void AddAttributesFromTypes(FilteredRequestType? types)
        {
            if (!types.HasValue)
            {
                return;
            }

            int count = _defaults.GetAttributeCount(types.Value, includeGlobal: false);
            if (count <= 0)
            {
                return;
            }

            string[] array = ArrayPool<string>.Shared.Rent(count);
            Span<string> attributes = array.AsSpan(0, count);

            _defaults.TryGetAllAttributes(types.Value, attributes, includeGlobal: false, out count);

            foreach (string s in attributes.Slice(0, count))
            {
                _request.Attributes.Add(s);
            }

            ArrayPool<string>.Shared.Return(array);
        }
        public LdapConnection ApplyConnection([NotNull] ref string? domain, IConnectionService connectionService)
        {
            domain ??= string.Empty;

            if (!connectionService.RegisteredConnections.TryGetValue(domain, out ConnectionContext? context))
            {
                throw new ArgumentException("Domain not registered.");
            }

            this.SearchBase = context.DefaultNamingContext;
            return context.CreateConnection();
        }
        private void RemoveDefaultAttributes()
        {
            if (!_hasDefaults)
            {
                return;
            }

            int i = _defaults.TotalGlobalAttributeCount - 1;
            for (;i >= 0; i--)
            {
                _request.Attributes.RemoveAt(i);
            }

            _hasDefaults = false;
        }
        /// <inheritdoc/>
        /// <remarks>
        /// Resets all search parameters and controls to their default values based on the global defaults.
        /// </remarks>
        protected override void ResetCore()
        {
            _requestId = Guid.Empty;
            ref readonly ISearchDefaults defaults = ref _defaults[string.Empty];
            _pageSize = 0;
            ResetRequest(_request, _pageControl, in defaults);
            _hasDefaults = true;
        }
        private static void ResetRequest<T>(SearchRequest request, StatedDirectoryControl<T> statedControl, ref readonly ISearchDefaults defaults) where T : DirectoryControl
        {
            request.Aliases = defaults.DereferenceAlias;
            request.Attributes.Clear();
            request.Attributes.AddRange(defaults.Attributes);
            request.DistinguishedName = string.Empty;
            request.Filter = string.Empty;
            request.Scope = defaults.Scope;
            request.SizeLimit = defaults.SizeLimit;
            request.TimeLimit = defaults.Timeout;

            statedControl.AddToRequest = false;
            statedControl.Reset();
            request.Controls.Add(statedControl);
        } 

        public SearchRequest AsLdapRequest()
        {
            return _request;
        }

        public byte[] GetCookie()
        {
            return _pageControl.GetControlValue(x => x.Cookie);
        }
        public void SetCookie(int? pageSize, byte[] cookie)
        {
            if (pageSize.HasValue)
            {
                this.PageSize = pageSize.Value;
                if (cookie.Length > 0)
                {
                    _pageControl.ChangeState(cookie, (bytes, control) => control.Cookie = bytes);
                }
            }
        }

        /// <inheritdoc/>
        bool IResettable.TryReset()
        {
            this.Reset();
            return true;
        }

        //public static implicit operator SearchRequest(LdapSearchRequest request)
        //{
        //    return request._request;
        //}
    }
}

