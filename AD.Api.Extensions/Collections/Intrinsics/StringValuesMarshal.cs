using Microsoft.Extensions.Primitives;

namespace AD.Api.Collections.Intrinsics;

public static class StringValuesMarshal
{
	public static object GetRawValue(StringValues value)
	{
		return RawValue.Get(value) ?? string.Empty;
	}

	private static class RawValue
	{
		internal static object? Get(StringValues value)
		{
			Holder holder = Unsafe.As<StringValues, Holder>(ref value);
			return holder._values;
		}

		[StructLayout(LayoutKind.Sequential)]
		private readonly struct Holder
		{
			internal readonly object? _values;
		}
	}
}
