namespace AD.Api.Strings.Spans;

public ref struct SplitAnyEnumerator
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private ReadOnlySpan<char> _current;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ReadOnlySpan<char> _splitBy;

	private ReadOnlySpan<char> _str;

	public readonly ReadOnlySpan<char> Current => _current;

	public SplitAnyEnumerator(ReadOnlySpan<char> str, ReadOnlySpan<char> splitByAny)
	{
		_splitBy = splitByAny;
		_str = str;
	}

	/// <summary>
	/// Returns the enumerator itself, as required by the compiler for the foreach syntax.
	/// </summary>
	/// <returns>The <see cref="SplitEnumerator"/> instance.</returns>
	public readonly SplitAnyEnumerator GetEnumerator() => this;

	public bool MoveNext()
	{
		ReadOnlySpan<char> span = _str;
		if (span.IsEmpty)
		{
			return false;
		}

		int index = span.IndexOfAny(_splitBy);
		if (index < 0)
		{
			_str = [];
			_current = new SplitEntry(span, default);
			return true;
		}

		_current = span.Slice(0, index);
		_str = index + 1 < span.Length ? span.Slice(index + 1) : [];

		return true;
	}
}