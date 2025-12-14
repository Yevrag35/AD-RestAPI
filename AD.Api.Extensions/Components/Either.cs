namespace AD.Api.Components;

/// <summary>
/// Provides static methods to create Either instances from remaining types.
/// </summary>
public static class Either
{
	/// <summary>
	/// The constant string representation of the Either type.
	/// </summary>
	internal const string TypeName = "Either<T1, T2>";

	/// <summary>
	/// Converts the first type of the given <see cref="Either{T1, T2}"/> instance when the union value
	/// is of type <typeparamref name="TStatic"/> to a
	/// new either union of type <typeparamref name="TTo"/>.
	/// </summary>
	/// <typeparam name="TFrom"></typeparam>
	/// <typeparam name="TTo"></typeparam>
	/// <typeparam name="TStatic"></typeparam>
	/// <param name="original"></param>
	/// <returns>
	/// A new either union with the first type is <typeparamref name="TTo"/> and the original <typeparamref name="TStatic"/> value.
	/// </returns>
	/// <exception cref="InvalidOperationException">The original union value is not of type <typeparamref name="TStatic"/>.</exception>
	public static Either<TTo, TStatic> ConvertTo<TFrom, TTo, TStatic>(in Either<TFrom, TStatic> original)
	{
		if (original.IsT1)
			throw new InvalidOperationException("Cannot convert a union value when the first type is not the static type.");

		return new Either<TTo, TStatic>(default!, original._second, original._index);
	}

	public static Either<T1, T2> FromT1<T1, T2>(T1 value) => new(value, default!, 1);
	public static Either<T1, T2> FromT1<T1, T2>(T1 value, [AllowNull] T2 unused) => new(value, default!, 1);
	public static Either<T1, T2> FromT2<T1, T2>(T2 value) => new(default!, value, 2);

	public static Either<T1?, T2> NullT1<T1, T2>() where T1 : class?
	{
		return new Either<T1?, T2>(null, default!, 1);
	}
	public static Either<T1, T2?> NullT2<T1, T2>() where T2 : class?
	{
		return new Either<T1, T2?>(default!, null, 2);
	}
	//public static Either<T1?, T2?> Null<T1, T2>(bool isT1) where T1 : class? where T2 : class?
	//{
	//	isT1 = !isT1; // Invert the boolean to match the index logic
	//	int index = Unsafe.As<bool, byte>(ref isT1) + 1;

	//	Debug.Assert(index == 1 && !isT1 || index == 2 && isT1, "The index should be 1 for T1 or 2 for T2.");
	//	return new Either<T1?, T2?>(null, null, (uint)index);
	//}

	/// <summary>
	/// Creates an Either instance from the first and third types of the given Either.
	/// </summary>
	/// <typeparam name="T1">The first type.</typeparam>
	/// <typeparam name="T2">The second type.</typeparam>
	/// <typeparam name="T3">The third type.</typeparam>
	/// <param name="remaining">The original Either instance.</param>
	/// <returns>A new Either instance containing the first and third types.</returns>
	internal static Either<T1, T3> FromRemainingOneAndThree<T1, T2, T3>(Either<T1, T2, T3> remaining)
	{
		Debug.Assert(remaining._index is 1 or 3, "The index should be either 1 or 3.");
		return new(remaining._first!, remaining._third!, remaining._index);
	}

	internal static Either<T1, T3, T4> FromRemainingOneThreeAndFour<T1, T2, T3, T4>(Either<T1, T2, T3, T4> original)
	{
		Debug.Assert(original._index is 1 or 3 or 4, "The index should be either 1, 3, or 4.");
		return new(in original._first!, in original._third!, in original._fourth!, original.Value!, original._index);
	}
	internal static Either<T1, T2, T3> FromRemainingOneTwoAndThree<T1, T2, T3, T4>(Either<T1, T2, T3, T4> original)
	{
		Debug.Assert(original._index is > 0 and < 4, "The index should be either 1, 2, or 3.");
		return new(in original._first!, in original._second!, in original._third!, original.Value!, original._index);
	}
	internal static Either<T1, T2, T4> FromRemainingOneTwoAndFour<T1, T2, T3, T4>(Either<T1, T2, T3, T4> original)
	{
		Debug.Assert(original._index is 1 or 2 or 4, "The index should be either 1, 2, or 4.");
		return new(in original._first!, in original._second!, in original._fourth!, original.Value!, original._index);
	}
	internal static Either<T2, T3, T4> FromRemainingTwoThreeAndFour<T1, T2, T3, T4>(Either<T1, T2, T3, T4> original)
	{
		Debug.Assert(original._index is >= 2 and < 5, "The index should be either 2, 3, or 4.");
		return new(in original._second!, in original._third!, in original._fourth!, original.Value!, original._index);
	}

	/// <summary>
	/// Creates an Either instance from the second and third types of the given Either.
	/// </summary>
	/// <typeparam name="T1">The first type.</typeparam>
	/// <typeparam name="T2">The second type.</typeparam>
	/// <typeparam name="T3">The third type.</typeparam>
	/// <param name="remaining">The original Either instance.</param>
	/// <returns>A new Either instance containing the second and third types.</returns>
	internal static Either<T2, T3> FromRemainingTwoAndThree<T1, T2, T3>(Either<T1, T2, T3> remaining)
	{
		Debug.Assert(remaining._index is 2 or 3, "The index should be either 2 or 3.");
		return new(remaining._second!, remaining._third!, remaining._index);
	}

	/// <summary>
	/// Creates an Either instance from the first and second types of the given Either.
	/// </summary>
	/// <typeparam name="T1">The first type.</typeparam>
	/// <typeparam name="T2">The second type.</typeparam>
	/// <typeparam name="T3">The third type.</typeparam>
	/// <param name="remaining">The original Either instance.</param>
	/// <returns>A new Either instance containing the first and second types.</returns>
	internal static Either<T1, T2> FromRemainingOneAndTwo<T1, T2, T3>(Either<T1, T2, T3> remaining)
	{
		Debug.Assert(remaining._index is 1 or 2, "The index should be either 1 or 2.");
		return new(remaining._first!, remaining._second!, remaining._index);
	}

	/// <summary>
	/// Flips the order of the types in the given Either instance.
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <param name="either"></param>
	/// <returns></returns>
	public static Either<T2, T1> Flip<T1, T2>(in Either<T1, T2> either)
	{
		uint mask = either._index - 1 >> 31 ^ 1;
		uint newIndex = (3 - either._index) * mask;

		Debug.Assert(newIndex is 0 or 1 or 2, "The new index should be either 0, 1, or 2.");
		return new Either<T2, T1>(either._second, either._first, newIndex);
	}
}