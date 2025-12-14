using System.ComponentModel;

namespace AD.Api.Components;

/// <summary>
/// Represents a union of four possible types.
/// </summary>
/// <typeparam name="T1">The first type.</typeparam>
/// <typeparam name="T2">The second type.</typeparam>
/// <typeparam name="T3">The third type.</typeparam>
/// <typeparam name="T4">The fourth type.</typeparam>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebugString(),nq}")]
public readonly partial struct Either<T1, T2, T3, T4>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly T1? _first;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly T2? _second;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly T3? _third;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly T4? _fourth;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	internal readonly byte _index;

	/// <summary>
	/// Gets the value of the first type if present.
	/// </summary>
	public readonly T1? AsT1 => _first;

	/// <summary>
	/// Gets the value of the second type if present.
	/// </summary>
	public readonly T2? AsT2 => _second;

	/// <summary>
	/// Gets the value of the third type if present.
	/// </summary>
	public readonly T3? AsT3 => _third;

	/// <summary>
	/// Gets the value of the fourth type if present.
	/// </summary>
	public readonly T4? AsT4 => _fourth;

	/// <summary>
	/// Gets the index of the current union indicating which type is present.
	/// </summary>
	public readonly byte Index => _index;

	/// <summary>
	/// Gets a value indicating whether the instance is default or empty.
	/// </summary>
	public readonly bool IsDefaultOrEmpty => _index == 0;

	/// <summary>
	/// Gets a value indicating whether the instance is of the first type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_first), nameof(AsT1))]
	public readonly bool IsT1 => _index == 1;

	/// <summary>
	/// Gets a value indicating whether the instance is of the second type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_second), nameof(AsT2))]
	public readonly bool IsT2 => _index == 2;

	/// <summary>
	/// Gets a value indicating whether the instance is of the third type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_third), nameof(AsT3))]
	public readonly bool IsT3 => _index == 3;

	/// <summary>
	/// Gets a value indicating whether the instance is of the fourth type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_fourth), nameof(AsT4))]
	public readonly bool IsT4 => _index == 4;

	/// <summary>
	/// Gets the value stored as a boxed object.
	/// </summary>
	public readonly object? Value { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2, T3, T4}"/> struct with the first type.
	/// </summary>
	/// <param name="first">The value of the first type.</param>
	private Either(T1 first)
	{
		_first = first;
		_second = default;
		_third = default;
		_fourth = default!;
		this.Value = first;
		_index = 1;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2, T3, T4}"/> struct with the second type.
	/// </summary>
	/// <param name="second">The value of the second type.</param>
	private Either(T2 second)
	{
		_first = default;
		_second = second;
		_third = default;
		_fourth = default!;
		this.Value = second;
		_index = 2;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2, T3, T4}"/> struct with the third type.
	/// </summary>
	/// <param name="third">The value of the third type.</param>
	private Either(T3 third)
	{
		_first = default;
		_second = default;
		_third = third;
		_fourth = default!;
		this.Value = third;
		_index = 3;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2, T3, T4}"/> struct with the fourth type.
	/// </summary>
	/// <param name="fourth">The value of the fourth type.</param>
	private Either(T4 fourth)
	{
		_first = default;
		_second = default;
		_third = default;
		_fourth = fourth;
		this.Value = fourth;
		_index = 4;
	}

	/// <summary>
	/// Matches the current instance to one of the provided actions based on its type.
	/// </summary>
	/// <typeparam name="TState">The state type.</typeparam>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	/// <param name="f3">The action to execute if the instance is of the third type.</param>
	public void Match<TState>(
		Action<T1> f1,
		Action<T2> f2,
		Action<T3> f3,
		Action<T4> f4)
	{
		switch (_index)
		{
			case 0:
				goto default;

			case 1:
				f1(_first!);
				break;

			case 2:
				f2(_second!);
				break;

			case 3:
				f3(_third!);
				break;

			case 4:
				f4(_fourth!);
				break;

			default:
				return;
		}
	}
	/// <summary>
	/// Matches the current instance to one of the provided actions based on its type, with state.
	/// </summary>
	/// <typeparam name="TState">The state type.</typeparam>
	/// <param name="state">The state to pass to the actions.</param>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	/// <param name="f3">The action to execute if the instance is of the third type.</param>
	public void Match<TState>(
		TState state,
		Action<T1, TState> f1,
		Action<T2, TState> f2,
		Action<T3, TState> f3,
		Action<T4, TState> f4) where TState : allows ref struct
	{
		switch (_index)
		{
			case 0:
				goto default;

			case 1:
				f1(_first!, state);
				break;

			case 2:
				f2(_second!, state);
				break;

			case 3:
				f3(_third!, state);
				break;

			case 4:
				f4(_fourth!, state);
				break;

			default:
				return;
		}
	}

	/// <summary>
	/// Matches the current instance to one of the provided functions based on its type and returns the result.
	/// </summary>
	/// <typeparam name="TOutput">The output type.</typeparam>
	/// <param name="f1">The function to execute if the instance is of the first type.</param>
	/// <param name="f2">The function to execute if the instance is of the second type.</param>
	/// <param name="f3">The function to execute if the instance is of the third type.</param>
	/// <returns>The result of the executed function.</returns>
	public TOutput Match<TOutput>(
		Func<T1, TOutput> f1,
		Func<T2, TOutput> f2,
		Func<T3, TOutput> f3,
		Func<T4, TOutput> f4)
	{
		return _index switch
		{
			1 => f1(_first!),
			2 => f2(_second!),
			3 => f3(_third!),
			4 => f4(_fourth!),
			_ => default!,
		};
	}
	/// <summary>
	/// Matches the current instance to one of the provided functions based on its type and returns the result, with state.
	/// </summary>
	/// <typeparam name="TOutput">The output type.</typeparam>
	/// <typeparam name="TState">The state type.</typeparam>
	/// <param name="state">The state to pass to the functions.</param>
	/// <param name="f1">The function to execute if the instance is of the first type.</param>
	/// <param name="f2">The function to execute if the instance is of the second type.</param>
	/// <param name="f3">The function to execute if the instance is of the third type.</param>
	/// <returns>The result of the executed function.</returns>
	/// <exception cref="EmptyStructException"></exception>
	public TOutput Match<TOutput, TState>(
		TState state,
		Func<T1, TState, TOutput> f1,
		Func<T2, TState, TOutput> f2,
		Func<T3, TState, TOutput> f3,
		Func<T4, TState, TOutput> f4) where TState : allows ref struct
	{
		return _index switch
		{
			1 => f1(_first!, state),
			2 => f2(_second!, state),
			3 => f3(_third!, state),
			4 => f4(_fourth!, state),
			0 or _ => default!,
		};
	}

	/// <summary>
	/// Tries to get the value of the first type.
	/// </summary>
	/// <param name="t1">The value of the first type if present.</param>
	/// <param name="remaining">The remaining Either instance containing the second and third types.</param>
	/// <returns><see langword="true"/> if the instance is of the first type, otherwise <see langword="false"/>.</returns>
	/// <exception cref="EmptyStructException"></exception>
	public readonly bool TryGetT1([NotNullWhen(true)] out T1? t1, out Either<T2, T3, T4> remaining)
	{
		t1 = _first;
		remaining = Either.FromRemainingTwoThreeAndFour(this);

		return this.IsT1;
	}
	/// <summary>
	/// Tries to get the value of the second type.
	/// </summary>
	/// <param name="t2">The value of the second type if present.</param>
	/// <param name="remaining">The remaining Either instance containing the first and third types.</param>
	/// <returns><see langword="true"/> if the instance is of the second type, otherwise <see langword="false"/>.</returns>
	/// <exception cref="EmptyStructException"></exception>
	public readonly bool TryGetT2([NotNullWhen(true)] out T2? t2, [NotNullWhen(false)] out Either<T1, T3, T4> remaining)
	{
		t2 = _second;
		remaining = Either.FromRemainingOneThreeAndFour(this);

		return this.IsT2;
	}
	/// <summary>
	/// Tries to get the value of the third type.
	/// </summary>
	/// <param name="t3">The value of the third type if present.</param>
	/// <param name="remaining">The remaining Either instance containing the first and second types.</param>
	/// <returns><see langword="true"/> if the instance is of the third type, otherwise <see langword="false"/>.</returns>
	/// <exception cref="EmptyStructException"></exception>
	public readonly bool TryGetT3([NotNullWhen(true)] out T3? t3, [NotNullWhen(false)] out Either<T1, T2, T4> remaining)
	{
		t3 = _third;
		remaining = Either.FromRemainingOneTwoAndFour(this);

		return this.IsT3;
	}
	/// <summary>
	/// Attempts to retrieve the value of the fourth type, <typeparamref name="T4"/>, if it is present.
	/// </summary>
	/// <param name="t4">When this method returns <see langword="true"/>, contains the value of type <typeparamref name="T4"/>. When this
	/// method returns <see langword="false"/>, the value is <see langword="null"/>.</param>
	/// <param name="remaining">When this method returns <see langword="false"/>, contains the remaining value as an <see cref="Either{T1, T2,
	/// T3}"/>. When this method returns <see langword="true"/>, the value is <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the value of type <typeparamref name="T4"/> is present; otherwise, <see
	/// langword="false"/>.</returns>
	public readonly bool TryGetT4([NotNullWhen(true)] out T4? t4, [NotNullWhen(false)] out Either<T1, T2, T3> remaining)
	{
		t4 = _fourth;
		remaining = Either.FromRemainingOneTwoAndThree(this);

		return this.IsT4;
	}

#if DEBUG
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Don't use this method - only for debugging display.", error: true)]
	private string GetDebugString()
	{
		return _index switch
		{
			1 => $"T1: {_first}",
			2 => $"T2: {_second}",
			3 => $"T3: {_third}",
			4 => $"T4: {_fourth}",
			_ => "Invalid",
		};
	}
#endif

	/// <summary>
	/// Implicitly converts a value of the first type to an Either instance.
	/// </summary>
	/// <param name="first">The value of the first type.</param>
	public static implicit operator Either<T1, T2, T3, T4>(T1 first) => new(first);
	/// <summary>
	/// Implicitly converts a value of the second type to an Either instance.
	/// </summary>
	/// <param name="second">The value of the second type.</param>
	public static implicit operator Either<T1, T2, T3, T4>(T2 second) => new(second);
	/// <summary>
	/// Implicitly converts a value of the third type to an Either instance.
	/// </summary>
	/// <param name="third">The value of the third type.</param>
	public static implicit operator Either<T1, T2, T3, T4>(T3 third) => new(third);
	/// <summary>
	/// Implicitly converts a value of the fourth type to an Either instance.
	/// </summary>
	/// <param name="fourth">The value of the fourth type.</param>
	public static implicit operator Either<T1, T2, T3, T4>(T4 fourth) => new(fourth);
}