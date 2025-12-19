namespace AD.Api.Components;

[StructLayout(LayoutKind.Sequential), DebuggerDisplay(@"\{Index = {_index}, Value = {_value}\}")]
public readonly partial struct ObjEither<T1, T2> where T1 : class where T2 : class
{
	internal readonly object? _value;
	private readonly uint _index;

	[MemberNotNullWhen(true, nameof(AsT1)), MemberNotNullWhen(false, nameof(AsT2))]
	public bool IsT1 => _index == 1;
	[MemberNotNullWhen(true, nameof(AsT2)), MemberNotNullWhen(false, nameof(AsT1))]
	public bool IsT2 => _index == 2;
	public uint Index => _index;
	public T1? AsT1 => (T1?)_value;
	public T2? AsT2 => (T2?)_value;

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

	public bool TryGetT1([NotNullWhen(true)] out T1? value, [NotNullWhen(false)] out T2? other)
	{
		switch (_index)
		{
			case 1:
				value = Unsafe.As<T1>(_value!);
				other = null;
				return true;

			case 2:
				value = null;
				other = Unsafe.As<T2>(_value!);
				return false;

			default:
				throw new InvalidOperationException("ObjEither must have a valid index");
		}
	}
	public bool TryGetT2([NotNullWhen(true)] out T2? value, [NotNullWhen(false)] out T1? other)
	{
		switch (_index)
		{
			case 1:
				other = Unsafe.As<T1>(_value!);
				value = null;
				return false;

			case 2:
				value = Unsafe.As<T2>(_value!);
				other = null;
				return true;

			default:
				throw new InvalidOperationException("ObjEither must have a valid index");
		}
	}

	public static implicit operator ObjEither<T1, T2>(T1 value) => new(value);
	public static implicit operator ObjEither<T1, T2>(T2 value) => new(value);
}
