using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Requests;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AD.Api.Core.Ldap;

/// <summary>
/// Represents a request to create an LDAP object.
/// </summary>
public abstract class CreateBody : ICreateRequest, IJsonOnDeserialized, IScopedRequest, IValidatableObject
{
	private readonly string? _path;

	/// <summary>
	/// The specified common name (cn) for the object.
	/// </summary>
	[JsonRequired]
	[JsonPropertyName("cn")]
	[MinLength(1, ErrorMessage = "Common names should always be at least 1 character in length.")]
	public required string CommonName { get; init; }
	/// <inheritdoc/>
	[MemberNotNullWhen(true, nameof(CommonName))]
	[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
	public bool HasPath { get; private set; }

	public string? Path
	{
		get => _path;
		init
		{
			_path = value;
			this.HasPath = !string.IsNullOrWhiteSpace(value);
		}
	}

	[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
	public abstract FilteredRequestType RequestType { get; }

	private DistinguishedName? _dn = DistinguishedName.Empty;
	/// <inheritdoc/>
	public DistinguishedName GetDistinguishedName()
	{
		return _dn ??= DistinguishedName.Empty;
	}

	protected abstract string GetScopedPathMemberName();
	string IScopedRequest.GetScopedPathMemberName()
	{
		return this.GetScopedPathMemberName();
	}
	protected virtual DistinguishedName GetScopedPath()
	{
		return this.GetDistinguishedName();
	}
	DistinguishedName IScopedRequest.GetScopedPath()
	{
		return this.GetScopedPath();
	}
	public void OnDeserialized()
	{
		DistinguishedName basePath = DistinguishedName.Parse(this.Path);
		RelativeNameType rdnType = this.RequestType switch
		{
			FilteredRequestType.OrganizationalUnit => RelativeNameType.OrganizationalUnit,
			_ => RelativeNameType.CommonName,
		};

		RelativeName rdn = RelativeName.Create(this.CommonName, rdnType);
		_dn = basePath.IsEmpty
			? new DistinguishedName(new ReadOnlySpan<RelativeName>(in rdn))
			: basePath.Insert(0, rdn);

		return;
	}
	/// <inheritdoc/>
	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		return [];
	}


}

