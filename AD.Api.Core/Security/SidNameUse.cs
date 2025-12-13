namespace AD.Api.Core.Security;

/// <summary>
/// Specifies the type and name usage of a security identifier (SID).
/// </summary>
public enum SidNameUse
{
	/// <summary>
	/// An undefined SID name use. This is never used and is just a placeholder.
	/// </summary>
	Undefined = 0,

	/// <summary>
	/// Indicates a user SID.
	/// </summary>
	User = 1,

	/// <summary>
	/// Indicates a group SID.
	/// </summary>
	Group = 2,

	/// <summary>
	/// Indicates a domain SID.
	/// </summary>
	Domain = 3,

	/// <summary>
	/// Indicates an alias SID.
	/// </summary>
	Alias = 4,

	/// <summary>
	/// Indicates a well-known group SID.
	/// </summary>
	WellKnownGroup = 5,

	/// <summary>
	/// Indicates a deleted account SID.
	/// </summary>
	DeletedAccount = 6,

	/// <summary>
	/// Indicates an invalid SID.
	/// </summary>
	Invalid = 7,

	/// <summary>
	/// Indicates an unknown SID type.
	/// </summary>
	Unknown = 8,

	/// <summary>
	/// Indicates a computer SID.
	/// </summary>
	Computer = 9,
}

