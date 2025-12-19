using AD.Api.Attributes.Services;
using AD.Api.Collections;
using AD.Api.Core.Extensions;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Settings;
using Microsoft.Extensions.ObjectPool;

namespace AD.Api.Core.Ldap;

[DependencyRegistration(Lifetime = ServiceLifetime.Transient)]
public sealed class LdapSearchRequest : LdapRequest, IResettable
{
	private const string DEFAULTS = "defaults";

	private static readonly string s_defaultRequestId = Guid.Empty.ToString();
	private readonly IDefaults _defaults;
	private readonly SearchRequest _request;
	private readonly LdapPropertyList _attributes;

	private bool _hasDefaults;
	protected override DirectoryRequest BackingRequest => _request;
	protected override string DefaultRequestId => s_defaultRequestId;

	public LdapPropertyList Attributes => _attributes;
	public int ControlCount => _request.Controls.Count;

	/// <summary>
	/// The <see cref="RequestId"/> contains the unique identifier for the LDAP request.
	/// </summary>
	/// <remarks>
	/// Each request will have its own RequestId per scoped request.
	/// </remarks>
	/// <returns>
	/// The requestID for the LDAP request as a <see cref="Guid"/> value.
	/// </returns>
	public Guid RequestId
	{
		[DebuggerStepThrough]
		get;
		set
		{
			if (value == Guid.Empty)
			{
				_request.RequestId = s_defaultRequestId;
				field = Guid.Empty;
			}
			else
			{
				_request.RequestId = value.ToString();
				field = value;
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
		_defaults = defaults;
		_request = new();
		_attributes = [];

		ISearchDefaults globals = _defaults[string.Empty];
		RequestMarshal.ReplaceAttributes(_request, _attributes);

		this.RequestId = Guid.Empty;
		ResetRequest(_request, _attributes, globals);
		_hasDefaults = globals.IsGlobal;
	}

	//public void AddAttributes(ReadOnlySpan<char> attributeString, FilteredRequestType? types)
	//{
	//	if (attributeString.IsWhiteSpace())
	//	{
	//		this.AddAttributesFromTypes(types);
	//		return;
	//	}

	//	bool wantsDefault = false;
	//	char separator = attributeString.Contains(CharConstants.COMMA) ? CharConstants.COMMA : CharConstants.SPACE;

	//	foreach (Range range in attributeString.Split(separator))
	//	{
	//		var section = attributeString[range];
	//		if (section.Equals(DEFAULTS.AsSpan(0, DEFAULTS.Length - 1), StringComparison.OrdinalIgnoreCase)
	//			||
	//			section.Equals(DEFAULTS, StringComparison.OrdinalIgnoreCase))
	//		{
	//			wantsDefault = true;
	//		}
	//		else if (!section.IsWhiteSpace())
	//		{
	//			string s = section.ToString();
	//			_ = _request.Attributes.Add(s);
	//		}
	//	}

	//	if (!wantsDefault)
	//	{
	//		this.RemoveDefaultAttributes();
	//	}
	//	else
	//	{
	//		this.AddAttributesFromTypes(types);
	//	}
	//}
	/// <summary>
	/// Adds the specified attribute properties to the current request, optionally including default attributes based on the
	/// provided values.
	/// </summary>
	/// <remarks>If any entry in the <paramref name="attributes"/> span indicates a request for default attributes,
	/// those defaults are added in addition to any explicitly specified attributes. If no such entry is present, any
	/// previously added default attributes are removed. Attribute names are added as provided; duplicate or invalid names
	/// are not filtered by this method.</remarks>
	/// <param name="attributes">A read-only span of attribute names to add. Each entry should be a non-empty, non-whitespace string. If the span is
	/// empty, only default attributes are considered.</param>
	/// <param name="types">An optional value specifying which types of default attributes to include if requested. If null, the method uses
	/// the default set of types.</param>
	public void AddAttributes(ReadOnlySpan<string> attributes, FilteredRequestType? types)
	{
		if (attributes.IsEmpty)
		{
			this.AddAttributesFromTypes(types);
			return;
		}

		bool wantsDefault = false;

		foreach (string att in attributes)
		{
			if (string.IsNullOrWhiteSpace(att))
			{
				continue;
			}

			ReadOnlySpan<char> working = att;
			if (working.StartsWith(DEFAULTS.AsSpan(0, DEFAULTS.Length - 1), StringComparison.OrdinalIgnoreCase))
			{
				working = working.Slice(DEFAULTS.Length - 1);

				if (working.IsEmpty || (working.Length == 1 && char.ToUpperInvariant(working[0]) == char.ToUpperInvariant(DEFAULTS[^1])))
				{
					wantsDefault = true;
					continue;
				}
			}

			_attributes.Add(att);
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

		_defaults.CopyTo(_attributes, types.Value);
	}
	protected override void OnApplyingContext(ConnectionContext context)
	{
		if (string.IsNullOrWhiteSpace(this.SearchBase))
		{
			this.SearchBase = context.DefaultNamingContext;
		}
	}
	private void RemoveDefaultAttributes()
	{
		if (!_hasDefaults)
		{
			return;
		}

		_attributes.RemoveAll(_defaults[string.Empty].Attributes);
		//int i = _defaults.TotalGlobalAttributeCount - 1;
		//for (; i >= 0; i--)
		//{
		//	_request.Attributes.RemoveAt(i);
		//}

		_hasDefaults = false;
	}
	/// <inheritdoc/>
	/// <remarks>
	/// Resets all search parameters and controls to their default values based on the global defaults.
	/// </remarks>
	protected override void ResetCore()
	{
		this.RequestId = Guid.Empty;
		ISearchDefaults defaults = _defaults[string.Empty];
		//_pageSize = 0;
		ResetRequest(_request, _attributes, defaults);
		_hasDefaults = true;
	}
	private static void ResetRequest(SearchRequest request, LdapPropertyList attributes, ISearchDefaults defaults)
	{
		request.Aliases = defaults.DereferenceAlias;
		attributes.Clear();
		attributes.AddRange(defaults.Attributes);
		request.DistinguishedName = string.Empty;
		request.Filter = string.Empty;
		request.Scope = defaults.Scope;
		request.SizeLimit = defaults.SizeLimit;
		request.TimeLimit = defaults.Timeout;
	}

	public SearchRequest AsLdapRequest()
	{
		return _request;
	}

	/// <inheritdoc/>
	bool IResettable.TryReset()
	{
		this.Reset();
		return true;
	}
}

