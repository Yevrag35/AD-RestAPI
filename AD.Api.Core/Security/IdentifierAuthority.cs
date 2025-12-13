namespace AD.Api.Core.Security;

/// <summary>
/// Specifies the identifier authorities for a security identifier (SID).
/// </summary>
public enum IdentifierAuthority : long
{
	/// <summary>
	/// Indicates a null authority.
	/// </summary>
	NullAuthority = 0,

	/// <summary>
	/// Indicates a world authority.
	/// </summary>
	WorldAuthority = 1,

	/// <summary>
	/// Indicates a local authority.
	/// </summary>
	LocalAuthority = 2,

	/// <summary>
	/// Indicates a creator authority.
	/// </summary>
	CreatorAuthority = 3,

	/// <summary>
	/// Indicates a non-unique authority.
	/// </summary>
	NonUniqueAuthority = 4,

	/// <summary>
	/// Indicates an NT authority.
	/// </summary>
	NTAuthority = 5,

	/// <summary>
	/// Indicates a site server authority.
	/// </summary>
	SiteServerAuthority = 6,

	/// <summary>
	/// Indicates an internet site authority.
	/// </summary>
	InternetSiteAuthority = 7,

	/// <summary>
	/// Indicates an exchange authority.
	/// </summary>
	ExchangeAuthority = 8,

	/// <summary>
	/// Indicates a resource manager authority.
	/// </summary>
	ResourceManagerAuthority = 9,
}
