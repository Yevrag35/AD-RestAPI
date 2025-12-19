namespace AD.Api.Components;

public readonly partial struct ObjEither<T1, T2>
{
	public TOutput Match<TOutput>(
		Func<T1, TOutput> f1,
		Func<T2, TOutput> f2)
	{
		return _index switch
		{
			1 => f1(Unsafe.As<T1>(_value)!),
			2 => f2(Unsafe.As<T2>(_value)!),
			_ => default!,
		};
	}
	public TOutput Match<TState, TOutput>(
		TState state,
		Func<TState, T1, TOutput> f1,
		Func<TState, T2, TOutput> f2) where TState : allows ref struct
	{
		return _index switch
		{
			1 => f1(state, Unsafe.As<T1>(_value)!),
			2 => f2(state, Unsafe.As<T2>(_value)!),
			_ => default!,
		};
	}
}
