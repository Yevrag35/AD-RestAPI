using AD.Api.Enums;

namespace AD.Api.Core.Ldap.Enums;

public static class UserAccountControlToStringExtensions
{
	private const int MAX_STACK = 512;

	public static int CopyTo(this UserAccountControl flags, Span<char> destination)
	{
		FlagEnumerator<UserAccountControl> enumerator = new(flags);
		if (!enumerator.MoveNext())
		{
			return 0;
		}

		if (!TryAppendEnum(enumerator.Current, destination, out int written))
			return 0;

		ReadOnlySpan<char> separator = [',', ' '];

		while (enumerator.MoveNext())
		{
			if (!separator.TryCopyTo(destination[written..], out int sepWritten))
			{
				return written + sepWritten;
			}

			written += sepWritten;
			if (!TryAppendEnum(enumerator.Current, destination[written..], out sepWritten))
			{
				return written + sepWritten;
			}

			written += sepWritten;
		}

		return written;
	}
	public static string ToFlaggedString(this UserAccountControl flags)
	{
		int maxLength = UserAccountControlEnumToStringExtensions.GetMaxToStringFastLength();
		using (var buffer = RentedBuffer.Rent<char>(
			maxLength <= MAX_STACK
				? stackalloc char[maxLength]
				: maxLength))
		{
			FlagEnumerator<UserAccountControl> enumerator = new(flags);
			if (!enumerator.MoveNext())
				return string.Empty;

			SpanStringBuilder builder = new(buffer.Span);
			try
			{
				AppendEnum(ref builder, enumerator.Current);
				while (enumerator.MoveNext())
				{
					builder.AppendChars(',', ' ');
					AppendEnum(ref builder, enumerator.Current);
				}

				return builder.ToString();
			}
			finally
			{
				builder.Dispose();
			}
		}
	}

	private static bool TryAppendEnum(UserAccountControl value, Span<char> destination, out int written)
	{
		if (value.TryGetName(out string? name))
		{
			written = name.Length;
			return name.TryCopyTo(destination);

		}
		else
		{
			return Enum.TryFormat(value, destination, out written);
		}
	}
	private static void AppendEnum(ref SpanStringBuilder builder, UserAccountControl value)
	{
		if (value.TryGetName(out string? name))
		{
			builder.Append(name);
		}
		else
		{
			builder.Append(value);
		}
	}
}
