namespace AD.Api.Buffers;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay(@"{Item1}, {Item2}")]
internal ref struct ByRefTuple<T1, T2>
{
	/// <summary>
	/// The first element of the tuple.
	/// </summary>
	public ref T1 Item1;
	/// <summary>
	/// The second element of the tuple.
	/// </summary>
	public ref T2 Item2;

	public ByRefTuple(ref T1 item1, ref T2 item2)
	{
		Item1 = ref item1;
		Item2 = ref item2;
	}
}
