namespace AD.Api.Statics;

/// <summary>
/// Provides commonly used constants related to GUID (Globally Unique Identifier) formatting and byte length.
/// </summary>
public static class GuidConstants
{
	/// <summary>
	/// Represents the number of bytes in a standard GUID value.
	/// </summary>
	/// <remarks>Use this constant when allocating buffers or performing operations that require the exact byte
	/// length of a GUID. A GUID is always 16 bytes in length according to the standard format.</remarks>
	public const int GUID_BYTE_LENGTH = 16;
	//public const string B_FORMAT = "B";
	/// <summary>
	/// The "D" format specifier represents a GUID in the standard 32-digit hexadecimal format with hyphens.
	/// </summary>
	public const string D_FORMAT = "D";
	/// <summary>
	/// The "N" format specifier represents a GUID as a 32-digit hexadecimal number without hyphens.
	/// </summary>
	public const string N_FORMAT = "N";
	public const string P_FORMAT = "P";
	public const string X_FORMAT = "X";
}