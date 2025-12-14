namespace AD.Api.Components;

[DebuggerStepThrough, Obsolete("Use Either<T1, T2, T3> instead.")]
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay(@"\{IsT0={IsT0}, IsT1={IsT1}, IsT2={IsT2}, Value={Value}\}")]
public readonly struct OneOf<T0, T1, T2>
{
	private readonly int _index;

	public readonly T0? AsT0 { get; }
	public readonly T1? AsT1 { get; }
	public readonly T2? AsT2 { get; }
	[MemberNotNullWhen(true, nameof(AsT0))]
	public readonly bool IsT0 { get; }
	[MemberNotNullWhen(true, nameof(AsT1))]
	public readonly bool IsT1 { get; }
	[MemberNotNullWhen(true, nameof(AsT2))]
	public readonly bool IsT2 { get; }

	public readonly int Index => _index;
	public readonly object Value { get; }

	private OneOf(object obj)
	{
		this.Value = obj;
	}
	internal OneOf(OneOf<T0, T1> oneOfT0T1)
	{
		_index = oneOfT0T1.Index;
		this.Value = oneOfT0T1.Value;
		this.AsT1 = oneOfT0T1.AsT1;
		this.AsT0 = oneOfT0T1.AsT0;
		this.AsT2 = default;
		this.IsT0 = oneOfT0T1.IsT0;
		this.IsT1 = oneOfT0T1.IsT1;
		this.IsT2 = false;
	}
	internal OneOf(OneOf<T0, T2> oneOfT0T2)
	{
		_index = oneOfT0T2.Index * 2;
		this.Value = oneOfT0T2.Value;
		this.AsT2 = oneOfT0T2.AsT1;
		this.AsT0 = oneOfT0T2.AsT0;
		this.AsT1 = default;

		this.IsT0 = oneOfT0T2.IsT0;
		this.IsT1 = false;
		this.IsT2 = oneOfT0T2.IsT1;
	}
	internal OneOf(OneOf<T1, T2> oneOfT1T2)
	{
		_index = oneOfT1T2.Index + 1;
		this.Value = oneOfT1T2.Value;
		this.AsT2 = oneOfT1T2.AsT1;
		this.AsT1 = oneOfT1T2.AsT0;
		this.AsT0 = default;

		this.IsT0 = false;
		this.IsT1 = oneOfT1T2.IsT0;
		this.IsT2 = oneOfT1T2.IsT1;
	}
	public OneOf(T0 t0Item)
		: this(obj: CastToObject(t0Item))
	{
		_index = 0;
		this.AsT0 = t0Item;
		this.IsT0 = true;

		this.AsT1 = default;
		this.IsT1 = false;
		this.AsT2 = default;
		this.IsT2 = false;
	}
	public OneOf(T1 t1Item)
		: this(obj: CastToObject(t1Item))
	{
		_index = 1;
		this.AsT1 = t1Item;
		this.IsT1 = true;

		this.AsT0 = default;
		this.IsT0 = false;
		this.AsT2 = default;
		this.IsT2 = false;
	}
	public OneOf(T2 t2Item)
		: this(obj: CastToObject(t2Item))
	{
		_index = 2;
		this.AsT2 = t2Item;
		this.IsT2 = true;

		this.AsT1 = default;
		this.IsT1 = false;
		this.AsT0 = default;
		this.IsT0 = false;
	}

	private static object CastToObject<T>(T item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return item;
	}

	public void Match<TState>(TState state, Action<TState, T0> a0, Action<TState, T1> a1, Action<TState, T2> a2)
	{
		switch (_index)
		{
			case 0:
				a0(state, this.AsT0!);
				break;

			case 1:
				a1(state, this.AsT1!);
				break;

			case 2:
				a2(state, this.AsT2!);
				break;

			default:
				break;
		}
	}

	public TOutput Match<TState, TOutput>(TState state, Func<TState, T0, TOutput> a0, Func<TState, T1, TOutput> a1, Func<TState, T2, TOutput> a2)
	{
		return _index switch
		{
			0 => a0(state, this.AsT0!),
			1 => a1(state, this.AsT1!),
			2 => a2(state, this.AsT2!),
			_ => throw new ArgumentOutOfRangeException("Index out of range", nameof(this.Index)),
		};
	}

	public bool TryGetT0([NotNullWhen(true)] out T0? t0, out OneOf<T1, T2> remainder)
	{
		t0 = this.AsT0;
		remainder = new(this.AsT1, this.AsT2, _index);

		return this.IsT0;
	}
	public bool TryGetT1([NotNullWhen(true)] out T1? t1, out OneOf<T0, T2> remainder)
	{
		t1 = this.AsT1;
		remainder = new(this.AsT0, this.AsT2, _index);

		return this.IsT1;
	}
	public bool TryGetT2([NotNullWhen(true)] out T2? t2, out OneOf<T0, T1> remainder)
	{
		t2 = this.AsT2;
		remainder = new(this.AsT0, this.AsT1, _index);

		return this.IsT2;
	}

	public static implicit operator OneOf<T0, T1, T2>(T0 t0Item) => new(t0Item);
	public static implicit operator OneOf<T0, T1, T2>(T1 t1Item) => new(t1Item);
	public static implicit operator OneOf<T0, T1, T2>(T2 t2Item) => new(t2Item);
	public static implicit operator OneOf<T0, T1, T2>(OneOf<T0, T1> oneOf) => new(oneOfT0T1: oneOf);
	public static implicit operator OneOf<T0, T1, T2>(OneOf<T0, T2> oneOf) => new(oneOfT0T2: oneOf);
	public static implicit operator OneOf<T0, T1, T2>(OneOf<T1, T2> oneOf) => new(oneOfT1T2: oneOf);
}