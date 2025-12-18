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

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay(@"{Item1}, {Item2}, {Item3}")]
internal ref struct ByRefTupleOne<T1, T2, T3> where T1 : allows ref struct where T2 : allows ref struct
{
	public readonly T1 Item1;
	public readonly T2 Item2;
	public ref T3 Item3;

	internal ByRefTupleOne(T1 item1, T2 item2, ref T3 item3)
	{
		Item1 = item1;
		Item2 = item2;
		Item3 = ref item3;
	}

	public void Deconstruct(out T1 item1, out T2 item2)
	{
		item1 = Item1;
		item2 = Item2;
	}
}