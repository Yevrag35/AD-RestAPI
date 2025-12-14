using AD.Api.Components;
using AD.Api.Statics;
using System.Globalization;

namespace AD.Api.Unmanaged;

/// <summary>
/// Extension methods for parsing FILETIME values from string or <see cref="long"/> objects.
/// </summary>
public static class FileTimeExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="value"></param>
	/// <param name="fileTime"></param>
	/// <returns></returns>
	public static bool TryGetFileTime(this ReadOnlySpan<char> value, out long fileTime)
	{
		return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out fileTime)
			&& !long.IsNegative(fileTime)
			&& LengthConstants.MaximumFileTime >= fileTime;
	}
	[DebuggerStepThrough]
	public static bool TryGetFileTime(this long value, out DateTimeOffset offset)
	{
		return TryGetFileTimeCore(in value, isLocal: true, out offset);
	}
	[DebuggerStepThrough]
	public static bool TryGetFileTimeUtc(this long value, out DateTimeOffset offset)
	{
		return TryGetFileTimeCore(in value, isLocal: false, out offset);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="value"></param>
	/// <param name="oneOf"></param>
	/// <returns></returns>
	[DebuggerStepThrough]
	public static bool TryGetFileTimeOrLong(this ReadOnlySpan<char> value, out OneOf<DateTimeOffset, long> oneOf)
	{
		return TryGetFileTimeOrLongCore(value, isLocal: true, out oneOf);
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="value"></param>
	/// <param name="oneOf"></param>
	/// <returns></returns>
	[DebuggerStepThrough]
	public static bool TryGetFileTimeOrLongUtc(this ReadOnlySpan<char> value, out OneOf<DateTimeOffset, long> oneOf)
	{
		return TryGetFileTimeOrLongCore(value, isLocal: false, out oneOf);
	}
	public static bool TryGetFileTimeOrLongCore(ReadOnlySpan<char> value, bool isLocal, out OneOf<DateTimeOffset, long> oneOf)
	{
		if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long longVal))
		{
			oneOf = default;
			return false;
		}

		oneOf = TryGetFileTimeCore(in longVal, isLocal, out DateTimeOffset offset)
			? offset
			: longVal;

		return true;
	}
	[DebuggerStepThrough]
	private static DateTimeOffset GetDateTimeOffset(long fileTime, bool isLocal)
	{
		return isLocal
			? DateTime.FromFileTime(fileTime)
			: DateTime.FromFileTimeUtc(fileTime);
	}
	private static bool TryGetFileTimeCore(in long fileTime, bool isLocal, out DateTimeOffset offset)
	{
		if (!long.IsNegative(fileTime) && LengthConstants.MaximumFileTime >= fileTime)
		{
			offset = GetDateTimeOffset(fileTime, isLocal);
			return true;
		}
		else
		{
			offset = default;
			return false;
		}
	}
}
