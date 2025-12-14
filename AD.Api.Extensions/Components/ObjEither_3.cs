namespace AD.Api.Components;

[StructLayout(LayoutKind.Sequential), DebuggerDisplay(@"\{Index = {_index}, Value = {_value}\}")]
public readonly struct ObjEither<T1, T2, T3> where T1 : class where T2 : class where T3 : class
{
	internal readonly object? _value;
	private readonly uint _index;

	public bool IsT1 => _index == 1;
	public bool IsT2 => _index == 2;
	public bool IsT3 => _index == 3;
	public uint Index => _index;
	public T1? AsT1 => (T1?)_value;
	public T2? AsT2 => (T2?)_value;
	public T3? AsT3 => (T3?)_value;
	public object? Value => _value;

	public ObjEither(T1 value)
	{
		_value = value;
		_index = 1;
	}

	public ObjEither(T2 value)
	{
		_value = value;
		_index = 2;
	}
	public ObjEither(T3 value)
	{
		_value = value;
		_index = 3;
	}
	private ObjEither(object? value, uint index)
	{
		ArgumentOutOfRangeException.ThrowIfZero(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 3u);
		_value = value;
		_index = index;
	}



	public static implicit operator ObjEither<T1, T2, T3>(T1 value) => new(value);
	public static implicit operator ObjEither<T1, T2, T3>(T2 value) => new(value);
	public static implicit operator ObjEither<T1, T2, T3>(T3 value) => new(value);

	public static implicit operator ObjEither<T1, T2, T3>(ObjEither<T1, T2> fromTwo)
	{
		return new ObjEither<T1, T2, T3>(fromTwo._value, fromTwo.Index);
	}
	public static implicit operator ObjEither<T1, T2, T3>(ObjEither<T1, T3> fromTwo)
	{
		uint index = fromTwo.Index == 2 ? 3 : fromTwo.Index;
		return new ObjEither<T1, T2, T3>(fromTwo._value, index);
	}
	public static implicit operator ObjEither<T1, T2, T3>(ObjEither<T2, T3> fromTwo)
	{
		ArgumentOutOfRangeException.ThrowIfZero(fromTwo.Index);
		uint index = fromTwo.Index + 1;
		return new ObjEither<T1, T2, T3>(fromTwo._value, index);
	}
}
