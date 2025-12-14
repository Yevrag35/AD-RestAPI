using AD.Api.Statics;
using System.Numerics;

namespace AD.Api.Unmanaged;

/// <summary>
/// Provides extension methods for unmanaged types.
/// </summary>
public static class NumberExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int MaxLength(this long _) => LengthConstants.LONG_MAX;

	/// <summary>Returns the number of UTF‑16 characters required to format the value in decimal.</summary>
	/// <remarks>Works for any signed or unsigned integer type that implements <see cref="IBinaryInteger{T}"/>.</remarks>
	public static int GetLength<T>(this T value) where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
	{
		// Fast‑track zero and ±1 — these dominate in many counters/state machines.
		if (T.IsZero(value) || T.One.Equals(value))
			return 1;

		// Track negativity without branching.  We need it only for the sign char.
		// (-x)  -> negativeSign = 1,  absValue = ~x + 1
		// ( x)  -> negativeSign = 0,  absValue =  x
		bool isNegative = T.IsNegative(value);
		int negativeSign = Unsafe.As<bool, byte>(ref isNegative);
		T absValue = isNegative
				   ? T.MinValue.Equals(value) ? T.MaxValue : T.Abs(value)
				   : value;

		// ---------------------------------------------------------------------
		// core digit‑count (works up to 256‑bit BigInteger without overflow)
		// ---------------------------------------------------------------------

		// log2(x) ≈ bitLength - 1;  Digits ≈ ceil(log10(x))
		// A very good integer approximation: digits = ((bitLen * 1233) >> 12) + 1
		//   1233 / 2^12  ≈ 0.30103  (log10(2))
		int bitLen = absValue.GetShortestBitLength();
		int digits = (bitLen * 1233 >> 12) + 1;

		// Correct the occasional +1 error without branching.
		// If absValue < 10^(digits-1) we must subtract 1.
		// 10^(digits-1) fits into BigInteger via Pow10 table; for smaller T we
		// use compile‑time constants so no heap alloc occurs.
		ref readonly T lookup = ref Pow10<T>.Get(digits - 1);
		if (lookup > absValue)
			digits--;

		return digits + negativeSign;
	}

	// -------------------------------------------------------------------------
	// Small lookup for 10^n so we can avoid BigInteger.Pow at runtime.
	// For n >= 39 we fall back to T.Pow, which is still branch‑free but slower.
	// -------------------------------------------------------------------------
	private static class Pow10<T> where T : unmanaged, IBinaryInteger<T>
	{
		private static readonly T[] s_table;

		internal static ref readonly T Get(int index) => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(s_table), index);

		static Pow10()
		{
			s_table = new T[40];   //  10^0 .. 10^39  fits in 128 bits
			s_table[0] = T.One;
			for (int i = 1; i < s_table.Length; i++)
			{
				s_table[i] = s_table[i - 1] * T.CreateTruncating(10);
			}
		}
	}
}