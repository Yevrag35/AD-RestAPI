namespace AD.Api.Buffers;

/// <summary>
/// Provides factory methods for creating ref tuple instances containing ref struct values.
/// </summary>
/// <remarks>
/// This static class includes methods to create <see cref="RefTuple{T1, T2}"/>, <see cref="RefTuple{T1, T2, T3}"/>,
/// and <see cref="UnsafeRefTuple{T1, T2}"/> instances. The type parameters in these methods are constrained with the
/// <c>allows ref struct</c> clause to ensure that ref structs can be used.
/// </remarks>
[DebuggerStepThrough]
public static class RefTuple
{
	///// <summary>
	///// Creates a <see cref="RefTuple{T1, T2}"/> instance from two readonly references.
	///// </summary>
	///// <typeparam name="T1">The type of the first element, constrained to allow ref struct types.</typeparam>
	///// <typeparam name="T2">The type of the second element, constrained to allow ref struct types.</typeparam>
	///// <param name="item1">A readonly reference to the first element.</param>
	///// <param name="item2">A readonly reference to the second element.</param>
	///// <returns>
	///// A new <see cref="RefTuple{T1, T2}"/> containing the specified elements.
	///// </returns>
	//public static RefTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) where T1 : allows ref struct where T2 : allows ref struct
	//{
	//	return new RefTuple<T1, T2>(item1, item2);
	//}
	///// <summary>
	///// Creates a <see cref="RefTuple{T1, T2, T3}"/> instance from three readonly references.
	///// </summary>
	///// <typeparam name="T1">The type of the first element, constrained to allow ref struct types.</typeparam>
	///// <typeparam name="T2">The type of the second element, constrained to allow ref struct types.</typeparam>
	///// <typeparam name="T3">The type of the third element, constrained to allow ref struct types.</typeparam>
	///// <param name="item1">A readonly reference to the first element.</param>
	///// <param name="item2">A readonly reference to the second element.</param>
	///// <param name="item3">A readonly reference to the third element.</param>
	///// <returns>
	///// A new <see cref="RefTuple{T1, T2, T3}"/> containing the specified elements.
	///// </returns>
	//public static RefTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) where T1 : allows ref struct where T2 : allows ref struct where T3 : allows ref struct
	//{
	//	return new RefTuple<T1, T2, T3>(item1, item2, item3);
	//}

	internal static ByRefTupleOne<T1, T2, T3> CreateByRefOne<T1, T2, T3>(T1 item1, T2 item2, ref T3 item3) where T1 : allows ref struct where T2 : allows ref struct
	{
		return new ByRefTupleOne<T1, T2, T3>(item1, item2, ref item3);
	}

	//public static TupleByRef<T1, T2> CreateByRef<T1, T2>(ref readonly T1 item1, ref readonly T2 item2)
	//{
	//	return new TupleByRef<T1, T2>(in item1, in item2);
	//}
	//public static TupleByRefOne<T1, T2> CreateByRefOne<T1, T2>(ref readonly T1 item1, ref readonly T2 item2) where T1 : struct where T2 : allows ref struct
	//{
	//	return new TupleByRefOne<T1, T2>(in item1, in item2);
	//}
	///// <summary>
	///// Creates an uninitialized <see cref="UnsafeRefTuple{T1, T2}"/> instance.
	///// </summary>
	///// <typeparam name="T1">The type of the first element, constrained to allow ref struct types.</typeparam>
	///// <typeparam name="T2">The type of the second element, constrained to allow ref struct types.</typeparam>
	///// <returns>
	///// A new, uninitialized <see cref="UnsafeRefTuple{T1, T2}"/>.
	///// </returns>
	///// <remarks>
	///// Since the tuple is created without initializing its elements, they may contain <see langword="null"/> values.
	///// Use with caution.
	///// </remarks>
	//public static UnsafeRefTuple<T1, T2> CreateUnsafe<T1, T2>() where T1 : allows ref struct where T2 : allows ref struct
	//{
	//	return new UnsafeRefTuple<T1, T2>();
	//}
}
