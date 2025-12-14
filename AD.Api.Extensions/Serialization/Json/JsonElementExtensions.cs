using System.Globalization;
using System.Numerics;

namespace AD.Api.Serialization.Json;

public static class JsonElementExtensions
{
	public static object GetNumber(this JsonElement element)
	{
		// 1) Exact integer fast paths (no allocation, no rounding)
		if (element.TryGetInt32(out int i32))
			return i32;
		if (element.TryGetInt64(out long i64))
			return i64;
		if (element.TryGetUInt64(out ulong u64))
			return u64;

		// 2) Decimal: exact for many non-scientific and up to 28-29 digits;
		// also exact for many integers beyond Int64/UInt64 range (but within decimal's range).
		if (element.TryGetDecimal(out decimal dec))
			return dec;

		// 3) Double: widest common floating range; may round but covers scientific notation well.
		if (element.TryGetDouble(out double dbl))
			return dbl;

		// 4) Lossless last resorts without direct access to the UTF-8 slice.
		// We must use GetRawText() (allocates a string) to decide BigInteger vs string.
		string raw = element.GetRawText();

		// If it looks like an integer (no '.' and no 'e/E'), try BigInteger.
		if (raw.AsSpan().IndexOfAny(['.', 'e', 'E']) == -1
			&& BigInteger.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out BigInteger big))
		{
			return big; // struct, boxes to ValueType as well
		}

		// 5) Give up on numeric boxing without losing information; preserve as text.
		return raw;
	}
}
