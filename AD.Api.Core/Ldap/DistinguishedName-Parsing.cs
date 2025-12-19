using System.Collections.Immutable;

namespace AD.Api.Core.Ldap;

public readonly partial struct DistinguishedName
{
	public static DistinguishedName Parse(ReadOnlySpan<char> distinguishedName)
	{
		if (distinguishedName.IsWhiteSpace())
		{
			return Empty;
		}

		int count = CountNumberOfRelativeNames(distinguishedName);
		RelativeName[] array = ArrayPool<RelativeName>.Shared.Rent(count);
		Span<RelativeName> span = array.AsSpan(0, count);

		if (!TrySplit(distinguishedName, span, out int namesWritten))
		{
			ArrayPool<RelativeName>.Shared.Return(array);
			return Empty;
		}

		span = span.Slice(0, namesWritten);
		DistinguishedName dn = new(span);
		ArrayPool<RelativeName>.Shared.Return(array);
		return dn;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"/>
	public static ImmutableArray<RelativeName> Split(ReadOnlySpan<char> path)
	{
		int count = CountNumberOfRelativeNames(path);
		RelativeName[] array = ArrayPool<RelativeName>.Shared.Rent(count);

		int start = 0;
		int n = 0;
		int i = 0;
		for (i = 0; i < path.Length; i++)
		{
			if (COMMA == path[i] && !path.IsEscapedAt(i))
			{
				ReadOnlySpan<char> slice = path.Slice(start, i - start);
				if (!RelativeName.TryParseOne(slice, out RelativeName rn))
				{
					throw new ArgumentException($"Invalid distinguished name component - make sure to escape any special characters: {slice.ToString()}", nameof(path));
				}

				array[n++] = rn;
				start = i + 1;
			}
		}

		if (start < path.Length)
		{
			ReadOnlySpan<char> slice = path.Slice(start);
			if (!RelativeName.TryParseOne(slice, out RelativeName last))
			{
				throw new ArgumentException($"Invalid distinguished name component - make sure to escape any special characters: {slice.ToString()}", nameof(path));
			}

			array[n++] = last;
		}

		ImmutableArray<RelativeName> result = ImmutableArray.Create(array.AsSpan(0, n));
		ArrayPool<RelativeName>.Shared.Return(array);

		return result;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="path"></param>
	/// <param name="destination"></param>
	/// <param name="namesWritten"></param>
	/// <returns></returns>
	public static bool TrySplit(ReadOnlySpan<char> path, Span<RelativeName> destination, out int namesWritten)
	{
		if (destination.IsEmpty)
		{
			namesWritten = 0;
			return false;
		}

		int start = 0;
		namesWritten = 0;
		int i = 0;
		for (i = 0; i < path.Length; i++)
		{
			if (COMMA == path[i] && !path.IsEscapedAt(i))
			{
				ReadOnlySpan<char> slice = path.Slice(start, i - start);
				if (!RelativeName.TryParseOne(slice, out RelativeName rn))
				{
					//Debug.Fail($"Invalid distinguished name component: {slice.ToString()}");
					return false;
				}

				destination[namesWritten++] = rn;
				start = i + 1;
			}
		}

		if (start < path.Length)
		{
			if (!RelativeName.TryParseOne(path.Slice(start), out RelativeName last))
			{
				//Debug.Fail($"Invalid distinguished name component: {last.ToString()}");
				return false;
			}

			destination[namesWritten++] = last;
		}

		return true;
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="path"></param>
	/// <param name="destination"></param>
	/// <param name="namesWritten"></param>
	/// <returns></returns>
	public static bool TrySplit(ReadOnlySpan<char> path, Span<RelativeName> destination, ref SpanStringBuilder erroredSections, out int namesWritten)
	{
		if (destination.IsEmpty)
		{
			namesWritten = 0;
			return false;
		}

		int start = 0;
		namesWritten = 0;
		bool failed = false;
		int i = 0;
		for (i = 0; i < path.Length; i++)
		{
			if (COMMA == path[i] && !path.IsEscapedAt(i))
			{
				ReadOnlySpan<char> slice = path.Slice(start, i - start);
				if (!RelativeName.TryParseOne(slice, out RelativeName rn))
				{
					erroredSections.AppendLine(slice);
					failed = true;
				}

				destination[namesWritten++] = rn;
				start = i + 1;
			}
		}

		if (start < path.Length)
		{
			ReadOnlySpan<char> slice = path.Slice(start);
			if (!RelativeName.TryParseOne(slice, out RelativeName last))
			{
				erroredSections.AppendLine(slice);
				failed = true;
			}

			destination[namesWritten++] = last;
		}

		return !failed;
	}
}