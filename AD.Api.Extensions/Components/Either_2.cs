using System.ComponentModel;

namespace AD.Api.Components;

/// <summary>
/// Represents a discriminated union between <typeparamref name="T1"/> and <typeparamref name="T2"/> where only one value is present at a time.
/// </summary>
/// <typeparam name="T1">The first possible type.</typeparam>
/// <typeparam name="T2">The second possible type.</typeparam>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("{GetDebugString(),nq}")]
public readonly partial struct Either<T1, T2>
{
	/// <summary>
	/// The value of the first type, if present.
	/// </summary>
	/// <remarks>
	/// Intended for internal use to store the first value in the union.
	/// </remarks>
	internal readonly T1 _first;

	/// <summary>
	/// The value of the second type, if present.
	/// </summary>
	/// <remarks>
	/// Intended for internal use to store the second value in the union.
	/// </remarks>
	internal readonly T2 _second;

	/// <summary>
	/// The internal index indicating which value is present.
	/// </summary>
	/// <remarks>
	/// 0 = default/empty, 1 = <typeparamref name="T1"/>, 2 = <typeparamref name="T2"/>.
	/// Used internally for efficient type discrimination.
	/// </remarks>
	internal readonly uint _index;

	/// <summary>
	/// Gets the value of the first type if present.
	/// </summary>
	public T1 AsT1 => _first;

	/// <summary>
	/// Gets the value of the second type if present.
	/// </summary>
	public T2 AsT2 => _second;

	/// <summary>
	/// Gets the index of the current union indicating which type is present.
	/// </summary>
	/// <value>
	/// Returns 1 if the value is of type <typeparamref name="T1"/>, 2 if of type <typeparamref name="T2"/>, or 0 if default-initialized.
	/// </value>
	public uint Index => _index;

	/// <summary>
	/// Gets a value indicating whether the instance is default-initialized.
	/// </summary>
	public bool IsDefault => _index == 0;

	/// <summary>
	/// Gets a value indicating whether the instance contains a value of the first type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_first), nameof(AsT1))]
	public bool IsT1 => _index == 1;

	/// <summary>
	/// Gets a value indicating whether the instance contains a value of the second type.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_second), nameof(AsT2))]
	public bool IsT2 => _index == 2;

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2}"/> struct with the specified values and index.
	/// </summary>
	/// <param name="first">The value of the first type.</param>
	/// <param name="second">The value of the second type.</param>
	/// <param name="index">The index indicating the current type (1 for <typeparamref name="T1"/>, 2 for <typeparamref name="T2"/>).</param>
	internal Either(T1 first, T2 second, uint index)
	{
		_first = first;
		_second = second;
		_index = index;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2}"/> struct with a value of the first type.
	/// </summary>
	/// <param name="first">The value of the first type.</param>
	private Either(T1 first)
	{
		_first = first;
		_second = default!;
		_index = 1;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Either{T1, T2}"/> struct with a value of the second type.
	/// </summary>
	/// <param name="second">The value of the second type.</param>
	private Either(T2 second)
	{
		_first = default!;
		_second = second;
		_index = 2;
	}

	/// <summary>
	/// Executes one of the provided actions based on the type currently held by this instance.
	/// </summary>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	public void Match(Action<T1> f1, Action<T2> f2)
	{
		switch (_index)
		{
			case 0:
				Debug.Fail("Empty struct");
				return;
			case 1:
				f1(_first!);
				break;
			case 2:
				f2(_second!);
				break;
			default:
				goto case 0;
		}
	}

	/// <summary>
	/// Executes one of the provided actions with state based on the type currently held by this instance.
	/// </summary>
	/// <typeparam name="TState">The type of the state parameter.</typeparam>
	/// <param name="state">The state to pass to the actions.</param>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	public void Match<TState>(
		TState state,
		Action<T1, TState> f1,
		Action<T2, TState> f2) where TState : allows ref struct
	{
		switch (_index)
		{
			case 0:
				Debug.Fail("Empty struct");
				break;
			case 1:
				f1(_first!, state);
				break;
			case 2:
				f2(_second!, state);
				break;
			default:
				goto case 0;
		}
	}

	/// <summary>
	/// Executes one of the provided functions based on the type currently held by this instance and returns the result.
	/// </summary>
	/// <typeparam name="TOutput">The output type.</typeparam>
	/// <param name="f1">The function to execute if the instance is of the first type.</param>
	/// <param name="f2">The function to execute if the instance is of the second type.</param>
	/// <returns>The result of the executed function, or the default value of <typeparamref name="TOutput"/> if the instance is empty.</returns>
	public TOutput Match<TOutput>(
		Func<T1, TOutput> f1,
		Func<T2, TOutput> f2)
	{
		return _index switch
		{
			1 => f1(_first),
			2 => f2(_second),
			_ => default!,
		};
	}

	/// <summary>
	/// Executes one of the provided functions with state based on the type currently held by this instance and returns the result.
	/// </summary>
	/// <typeparam name="TOutput">The output type.</typeparam>
	/// <typeparam name="TState">The type of the state parameter.</typeparam>
	/// <param name="state">The state to pass to the functions.</param>
	/// <param name="f1">The function to execute if the instance is of the first type.</param>
	/// <param name="f2">The function to execute if the instance is of the second type.</param>
	/// <returns>The result of the executed function.</returns>
	public TOutput Match<TOutput, TState>(
		TState state,
		Func<T1, TState, TOutput> f1,
		Func<T2, TState, TOutput> f2) where TState : allows ref struct
	{
		return _index switch
		{
			1 => f1(_first, state),
			2 => f2(_second, state),
			_ => throw new InvalidOperationException("Empty struct"),
		};
	}

	/// <summary>
	/// Executes one of the provided actions with an <see cref="object"/> state based on the type currently held by this instance.
	/// </summary>
	/// <remarks>
	/// This overload is provided for scenarios where the state is of type <see cref="object"/> to avoid delegate allocations.
	/// </remarks>
	/// <param name="state">The state to pass to the actions.</param>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	public readonly void MatchObj(
		object? state,
		Action<T1, object?> f1,
		Action<T2, object?> f2)
	{
		switch (_index)
		{
			case 0:
				Debug.Fail("Empty struct");
				break;
			case 1:
				f1(_first, state);
				break;
			case 2:
				f2(_second, state);
				break;
			default:
				Debug.Fail("Invalid index in Either struct");
				break;
		}
	}

	/// <summary>
	/// Executes one of the provided functions with an <see cref="object"/> state based on the type currently held by this instance and returns the result.
	/// </summary>
	/// <remarks>
	/// This overload is provided for scenarios where the state and return types are <see cref="object"/> to avoid delegate allocations.
	/// </remarks>
	/// <param name="state">The state to pass to the functions.</param>
	/// <param name="f1">The function to execute if the instance is of the first type.</param>
	/// <param name="f2">The function to execute if the instance is of the second type.</param>
	/// <returns>The result of the executed function, or <see langword="null"/> if the instance is empty.</returns>
	public readonly object? MatchObj(
		object? state,
		Func<T1, object?, object?> f1,
		Func<T2, object?, object?> f2)
	{
		return _index switch
		{
			1 => f1(_first, state),
			2 => f2(_second, state),
			_ => null,
		};
	}

	/// <summary>
	/// Executes one of the provided actions with a <see cref="ReadOnlySpan{T}"/> state based on the type currently held by this instance.
	/// </summary>
	/// <remarks>
	/// This overload is provided for scenarios where the state is a <see cref="ReadOnlySpan{T}"/> of <see cref="object"/> to avoid delegate allocations.
	/// </remarks>
	/// <param name="f1">The action to execute if the instance is of the first type.</param>
	/// <param name="f2">The action to execute if the instance is of the second type.</param>
	/// <param name="state">The state to pass to the actions.</param>
	public readonly void MatchObj(
		Action<T1, ReadOnlySpan<object?>> f1,
		Action<T2, ReadOnlySpan<object?>> f2,
		params ReadOnlySpan<object?> state)
	{
		switch (_index)
		{
			case 0:
				Debug.Fail("Empty struct");
				break;
			case 1:
				f1(_first, state);
				break;
			case 2:
				f2(_second, state);
				break;
			default:
				Debug.Fail("Invalid index in Either struct");
				break;
		}
	}

	/// <summary>
	/// Attempts to get the value of the first type.
	/// </summary>
	/// <param name="t1">When this method returns, contains the value of the first type if present; otherwise, <see langword="null"/>.</param>
	/// <param name="t2">When this method returns, contains the value of the second type if present; otherwise, <see langword="null"/>.</param>
	/// <returns>
	/// <see langword="true"/> if the instance is of the first type; otherwise, <see langword="false"/>.
	/// </returns>
	public bool TryGetT1([NotNullWhen(true)] out T1? t1, [NotNullWhen(false)] out T2? t2)
	{
		Debug.Assert(!this.IsDefault);
		t1 = _first;
		t2 = _second;
		return this.IsT1;
	}

	/// <summary>
	/// Attempts to get the value of the second type.
	/// </summary>
	/// <param name="t2">When this method returns, contains the value of the second type if present; otherwise, <see langword="null"/>.</param>
	/// <param name="t1">When this method returns, contains the value of the first type if present; otherwise, <see langword="null"/>.</param>
	/// <returns>
	/// <see langword="true"/> if the instance is of the second type; otherwise, <see langword="false"/>.
	/// </returns>
	public bool TryGetT2([NotNullWhen(true)] out T2 t2, [NotNullWhen(false)] out T1 t1)
	{
		Debug.Assert(!this.IsDefault);
		t1 = _first;
		t2 = _second;
		return this.IsT2;
	}

	/// <summary>
	/// Returns a string representation of the current value for debugging purposes.
	/// </summary>
	/// <remarks>
	/// This method is intended for debugger display only and should not be used in production code.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Don't use this method - only for debugging display.", error: true)]
	private string GetDebugString()
	{
		return _index switch
		{
			1 => $"T1: {_first}",
			2 => $"T2: {_second}",
			_ => "Invalid",
		};
	}

	/// <summary>
	/// Implicitly converts a value of the first type to an <see cref="Either{T1, T2}"/> instance.
	/// </summary>
	/// <param name="first">The value of the first type.</param>
	public static implicit operator Either<T1, T2>(T1 first) => new(first);

	/// <summary>
	/// Implicitly converts a value of the second type to an <see cref="Either{T1, T2}"/> instance.
	/// </summary>
	/// <param name="second">The value of the second type.</param>
	public static implicit operator Either<T1, T2>(T2 second) => new(second);
}
