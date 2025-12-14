namespace AD.Api.Statics;

/// <summary>
/// Provides constant values for various length constraints.
/// </summary>
public static class LengthConstants
{
	/// <summary>
	/// Maximum length for a byte value.
	/// </summary>
	public const int BYTE_MAX = 3;

	/// <summary>
	/// Maximum length for an integer value.
	/// </summary>
	public const int INT_MAX = 11;

	/// <summary>
	/// Maximum length for a long value.
	/// </summary>
	public const int LONG_MAX = 20;

	/// <summary>
	/// Maximum length for an unsigned integer value.
	/// </summary>
	public const int UINT_MAX = INT_MAX - 1;

	/// <summary>
	/// Maximum length for an unsigned long value.
	/// </summary>
	/// <value><c>20</c></value>
	public const int ULONG_MAX = LONG_MAX;

	/// <summary>
	/// Maximum length for a double value.
	/// </summary>
	public const int DOUBLE_MAX = 24;    // double.MinValue.ToString().Length

	/// <summary>
	/// Maximum length for a decimal value.
	/// </summary>
	public const int DECIMAL_MAX = 30;

	/// <summary>
	/// Maximum length for a 128-bit integer value.
	/// </summary>
	public const int INT128_MAX = 40;

	/// <summary>
	/// Length of a GUID in 'B' or 'P' format.
	/// </summary>
	public const int GUID_FORM_B_OR_P = 38;

	/// <summary>
	/// Length of a GUID in 'N' format.
	/// </summary>
	public const int GUID_FORM_N = 32;

	/// <summary>
	/// Length of a GUID in 'D' format.
	/// </summary>
	public const int GUID_FORM_D = 36;

	/// <summary>
	/// Length of a GUID in 'X' format.
	/// </summary>
	public const int GUID_FORM_X = 68;

	public const long MaximumFileTime = 2650467743999999999;

	/// <summary>
	/// Gets the length of a GUID based on the specified format character.
	/// </summary>
	/// <param name="format">The format character ('B', 'P', 'N', 'D', or 'X').</param>
	/// <returns>The length of the GUID in the specified format.</returns>
	/// <inheritdoc cref="GetGuidLengthCore(char)" path="/exception"/>
	public static int GetGuidLength(char format)
	{
		return GetGuidLengthCore(format);
	}
	/// <summary>
	/// Gets the length of a GUID based on the specified read-only span consisting of a single character.
	/// </summary>
	/// <param name="format">The read-only span that contains the single format character.</param>
	/// <returns><inheritdoc cref="GetGuidLength(char)"/></returns>
	/// <exception cref="ArgumentException">Thrown when the format is not a single character.</exception>
	/// <inheritdoc cref="GetGuidLengthCore(char)" path="/exception"/>
	public static int GetGuidLength(ReadOnlySpan<char> format)
	{
		return format.Length switch
		{
			0 => GUID_FORM_D, // Default format
			1 => GetGuidLengthCore(format[0]),
			_ => throw new ArgumentException("GUID formats must be only 1 character in length.", nameof(format)),
		};
	}
	/// <exception cref="FormatException">Thrown when the format character is invalid.</exception>
	private static int GetGuidLengthCore(char format)
	{
		return format switch
		{
			'B' or 'P' or 'b' or 'p' => GUID_FORM_B_OR_P,
			'N' or 'n' => GUID_FORM_N,
			'D' or 'd' => GUID_FORM_D,
			'X' or 'x' => GUID_FORM_X,
			_ => throw new FormatException($"Invalid GUID format specified -> {format}"),
		};
	}
}
